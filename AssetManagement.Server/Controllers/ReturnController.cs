using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Domain.entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/returns")]
    [Authorize]
    public class ReturnController : ControllerBase
    {
        private readonly AssetDbContext _context;

        public ReturnController(AssetDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "EMPLOYEE")]
        public async Task<IActionResult> Initiate([FromBody] InitiateReturnRequestDto dto)
        {
            var userId = ResolveCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token." });

            var assignment = await _context.Assignments
                .Include(a => a.Asset)
                .FirstOrDefaultAsync(a => a.Id == dto.AssignmentId && !a.IsDeleted);

            if (assignment == null)
                return NotFound(new { message = "Assignment not found." });

            if (assignment.UserId != userId.Value)
                return Forbid();

            if (assignment.AssetId != dto.AssetId)
                return BadRequest(new { message = "Asset does not belong to this assignment." });

            if (assignment.Asset != null &&
                (string.Equals(assignment.Asset.Status, AssetStatus.BEYOND_REPAIR.ToString(), StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(assignment.Asset.Status, AssetStatus.LIQUIDATED.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "This asset is no longer eligible for return workflow." });
            }

            if (!string.Equals(assignment.Status, "ASSIGNED", StringComparison.OrdinalIgnoreCase) ||
                assignment.ReturnedAt != null)
            {
                return BadRequest(new { message = "Assignment is not eligible for return." });
            }

            var hasOpenRequest = await _context.ReturnRequests.AnyAsync(r =>
                !r.IsDeleted &&
                r.AssignmentId == assignment.Id &&
                (r.Status == "REQUESTED" || r.Status == "PENDING_INSPECTION"));

            if (hasOpenRequest)
                return BadRequest(new { message = "An open return request already exists." });

            var now = DateTime.UtcNow;

            var returnRequest = new AssetReturnRequest
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignment.Id,
                AssetId = assignment.AssetId,
                UserId = assignment.UserId,
                InitiatedById = userId.Value,
                ReturnReason = dto.Reason,
                Notes = dto.Notes,
                ExpectedReturnDate = dto.ExpectedReturnDate,
                Status = "REQUESTED",
                CreatedAt = now
            };

            _context.ReturnRequests.Add(returnRequest);

            var previousStatus = assignment.Asset.Status;

            _context.AssetHistories.Add(new AssetHistory
            {
                Id = Guid.NewGuid(),
                AssetId = assignment.AssetId,
                AssignmentId = assignment.Id,
                UserId = assignment.UserId,
                ChangedById = userId.Value,
                Action = AssetHistoryActions.ReturnRequested,
                PreviousStatus = previousStatus,
                NewStatus = previousStatus,
                Note = dto.Reason,
                ChangedAt = now,
                CreatedAt = now
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = returnRequest.Id,
                assignmentId = returnRequest.AssignmentId,
                assetId = returnRequest.AssetId,
                assetTag = assignment.Asset.AssetTag,
                reason = returnRequest.ReturnReason,
                notes = returnRequest.Notes,
                status = returnRequest.Status,
                createdAt = returnRequest.CreatedAt
            });
        }

        [HttpPost("{id:guid}/receive")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Receive(Guid id, [FromBody] ReceiveReturnRequestDto dto)
        {
            var adminId = ResolveCurrentUserId();
            if (adminId == null)
                return Unauthorized(new { message = "Invalid token." });

            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId.Value);
            if (adminUser == null)
            {
                Console.WriteLine($"RETURN: admin user lookup by id {adminId} returned null. Trying email fallback.");
                // try resolve by email claim
                string? claimEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
                if (!string.IsNullOrWhiteSpace(claimEmail))
                {
                    adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == claimEmail);
                    if (adminUser != null)
                    {
                        adminId = adminUser.Id;
                        Console.WriteLine($"RETURN: resolved admin user by email {claimEmail} -> {adminId}");
                    }
                }

                if (adminUser == null)
                {
                    Console.WriteLine($"RETURN: admin user still not found for id {adminId}. Proceeding without setting handled_by/changed_by FKs.");
                }
            }

            if (!dto.PhysicallyReceived)
                return BadRequest(new { message = "Asset must be physically received first." });

            var returnRequest = await _context.ReturnRequests
                .Include(r => r.Assignment)
                .Include(r => r.Asset)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (returnRequest == null)
                return NotFound(new { message = "Return request not found." });

            if (returnRequest.Asset != null &&
                (string.Equals(returnRequest.Asset.Status, AssetStatus.BEYOND_REPAIR.ToString(), StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(returnRequest.Asset.Status, AssetStatus.LIQUIDATED.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "This asset is no longer eligible for return workflow." });
            }

            if (!string.Equals(returnRequest.Status, "REQUESTED", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Return request is not in REQUESTED state." });

            var now = dto.ReceivedAt ?? DateTime.UtcNow;
            var previousAssetStatus = returnRequest.Asset?.Status;

            returnRequest.Status = "PENDING_INSPECTION";
            returnRequest.ConditionAtHandback = dto.HandbackCondition;
            returnRequest.Notes = MergeNotes(returnRequest.Notes, dto.Notes);
            // attach navigation to ensure FK is tracked correctly
            returnRequest.HandledBy = adminUser;
            returnRequest.HandledAt = now;
            returnRequest.UpdatedAt = now;

            if (returnRequest.Assignment != null)
            {
                returnRequest.Assignment.ReturnedAt = now;
                returnRequest.Assignment.Status = AssignmentStatus.RETURNED.ToString();
                returnRequest.Assignment.UpdatedAt = now;
            }

            if (returnRequest.Asset != null)
            {
                returnRequest.Asset.CurrentUserId = null;
                returnRequest.Asset.Status = AssetStatus.AVAILABLE.ToString();
                returnRequest.Asset.UpdatedAt = now;

                _context.AssetHistories.Add(new AssetHistory
                {
                    Id = Guid.NewGuid(),
                    AssetId = returnRequest.Asset.Id,
                    AssignmentId = returnRequest.AssignmentId,
                    UserId = returnRequest.UserId,
                    ChangedById = adminUser?.Id,
                    Action = AssetHistoryActions.ReturnApproved,
                    PreviousStatus = previousAssetStatus,
                    NewStatus = returnRequest.Asset.Status,
                    Note = returnRequest.ReturnReason,
                    ChangedAt = now,
                    CreatedAt = now
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = returnRequest.Id,
                status = returnRequest.Status,
                handbackCondition = returnRequest.ConditionAtHandback,
                receivedAt = now,
                assetStatus = returnRequest.Asset?.Status
            });
        }

        [HttpPost("{id:guid}/inspect")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Inspect(Guid id, [FromBody] InspectReturnRequestDto dto)
        {
            try
            {
                var adminId = ResolveCurrentUserId();
                if (adminId == null)
                    return Unauthorized(new { message = "Invalid token." });

                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminId.Value);

                if (adminUser == null)
                {
                    Console.WriteLine($"RETURN INSPECT: admin user lookup by id {adminId} returned null. Trying email fallback.");

                    string? claimEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");

                    if (!string.IsNullOrWhiteSpace(claimEmail))
                    {
                        adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == claimEmail);

                        if (adminUser != null)
                        {
                            adminId = adminUser.Id;
                            Console.WriteLine($"RETURN INSPECT: resolved admin user by email {claimEmail} -> {adminId}");
                        }
                    }

                    if (adminUser == null)
                    {
                        return Unauthorized(new { message = "User not found." });
                    }
                }

                if (dto == null)
                {
                    return BadRequest(new { message = "Invalid payload." });
                }

                var returnRequest = await _context.ReturnRequests
                    .Include(r => r.Assignment)
                    .Include(r => r.Asset)
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (returnRequest == null)
                    return NotFound(new { message = "Return request not found." });

                if (returnRequest.Asset != null &&
                    (string.Equals(returnRequest.Asset.Status, AssetStatus.BEYOND_REPAIR.ToString(), StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(returnRequest.Asset.Status, AssetStatus.LIQUIDATED.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { message = "This asset is no longer eligible for return workflow." });
                }

                if (!string.Equals(returnRequest.Status, "PENDING_INSPECTION", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Return request is not in PENDING_INSPECTION state." });

                if (string.IsNullOrWhiteSpace(dto.InspectionResult) && string.IsNullOrWhiteSpace(dto.HandbackCondition))
                {
                    // At least one of these should be provided to determine final status / notes
                    return BadRequest(new { message = "inspectionResult or handbackCondition is required." });
                }

                var now = DateTime.UtcNow;
                var finalAssetStatus = ResolveFinalAssetStatus(dto.InspectionResult, dto.HandbackCondition ?? returnRequest.ConditionAtHandback);

                // Persist returnRequest and assignment first to ensure FK references are valid
                returnRequest.Status = "INSPECTED";
                returnRequest.ConditionAtHandback = dto.HandbackCondition ?? returnRequest.ConditionAtHandback;
                returnRequest.Notes = MergeNotes(returnRequest.Notes, dto.InspectionNotes, dto.AccessoriesNotes);
                // attach navigation to ensure FK is tracked correctly
                returnRequest.HandledBy = adminUser;
                returnRequest.HandledAt = now;
                returnRequest.UpdatedAt = now;

                if (returnRequest.Assignment != null)
                {
                    returnRequest.Assignment.ReturnedAt ??= now;
                    returnRequest.Assignment.Status = AssignmentStatus.RETURNED.ToString();
                    returnRequest.Assignment.UpdatedAt = now;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception saveEx)
                {
                    Console.WriteLine("RETURN Inspect SAVE STEP 1 ERROR: " + saveEx);
                    throw; // let outer catch handle and return 500 with detail
                }

                // Now update asset and histories in a separate step
                if (returnRequest.Asset != null)
                {
                    var previousStatus = returnRequest.Asset.Status;

                    returnRequest.Asset.Status = finalAssetStatus;
                    returnRequest.Asset.CurrentUserId = null;
                    returnRequest.Asset.UpdatedAt = now;

                    if (!string.Equals(previousStatus, finalAssetStatus, StringComparison.OrdinalIgnoreCase))
                    {
                        _context.AssetHistories.Add(new AssetHistory
                        {
                            Id = Guid.NewGuid(),
                            AssetId = returnRequest.Asset.Id,
                            AssignmentId = returnRequest.AssignmentId,
                            UserId = returnRequest.UserId,
                            ChangedById = adminUser.Id,
                            Action = AssetHistoryActions.ReturnInspected,
                            PreviousStatus = previousStatus,
                            NewStatus = finalAssetStatus,
                            Note = returnRequest.Notes,
                            ChangedAt = now,
                            CreatedAt = now
                        });
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception saveEx)
                    {
                        Console.WriteLine("RETURN Inspect SAVE STEP 2 ERROR: " + saveEx);
                        throw;
                    }
                }

                return Ok(new
                {
                    id = returnRequest.Id,
                    status = returnRequest.Status,
                    assetStatus = finalAssetStatus,
                    inspectedAt = now
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("RETURN Inspect ERROR: " + ex);
                return StatusCode(500, new { message = "Internal server error during inspect.", detail = ex.Message });
            }
        }

        [HttpGet("my")]
        [Authorize(Roles = "EMPLOYEE")]
        public async Task<IActionResult> GetMyReturns()
        {
            var userId = ResolveCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "Invalid token." });

            var items = await _context.ReturnRequests
                .AsNoTracking()
                .Include(r => r.Asset)
                .Include(r => r.Assignment)
                .Where(r => !r.IsDeleted && r.UserId == userId.Value)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var data = items.Select(r => new
            {
                id = r.Id,
                assignmentId = r.AssignmentId,
                assetId = r.AssetId,
                assetTag = r.Asset.AssetTag,
                assetName = r.Asset.Model,
                requestedById = r.InitiatedById,
                targetUserId = r.UserId,
                reason = r.ReturnReason,
                notes = r.Notes,
                status = r.Status,
                createdAt = r.CreatedAt,
                initiatedAt = r.CreatedAt,
                handbackCondition = r.ConditionAtHandback,
                receivedAt = r.Status == "REQUESTED" ? null : r.HandledAt,
                inspectedAt = r.Status == "INSPECTED" ? r.HandledAt : null
            });

            return Ok(data);
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _context.ReturnRequests
                .AsNoTracking()
                .Include(r => r.Asset)
                .Include(r => r.Assignment)
                    .ThenInclude(a => a.User)
                .Include(r => r.InitiatedBy)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var data = items.Select(r => new
            {
                id = r.Id,
                assignmentId = r.AssignmentId,
                assetId = r.AssetId,
                assetTag = r.Asset.AssetTag,
                assetName = r.Asset.Model,
                requestedById = r.InitiatedById,
                requestedByName = r.InitiatedBy.FullName ?? r.InitiatedBy.Email,
                targetUserId = r.UserId,
                targetUserName = r.Assignment?.User?.FullName ?? r.Assignment?.User?.Email,
                reason = r.ReturnReason,
                notes = r.Notes,
                status = r.Status,
                createdAt = r.CreatedAt,
                initiatedAt = r.CreatedAt,
                handbackCondition = r.ConditionAtHandback,
                receivedAt = r.Status == "REQUESTED" ? null : r.HandledAt,
                inspectedAt = r.Status == "INSPECTED" ? r.HandledAt : null
            });

            return Ok(data);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _context.ReturnRequests
                .AsNoTracking()
                .Include(r => r.Asset)
                .Include(r => r.Assignment)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (item == null)
                return NotFound(new { message = "Return request not found." });

            return Ok(new
            {
                id = item.Id,
                assignmentId = item.AssignmentId,
                assetId = item.AssetId,
                assetTag = item.Asset.AssetTag,
                assetName = item.Asset.Model,
                status = item.Status,
                reason = item.ReturnReason,
                notes = item.Notes,
                createdAt = item.CreatedAt,
                handbackCondition = item.ConditionAtHandback
            });
        }

        private Guid? ResolveCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(raw, out var userId))
                return userId;

            // Fallback: if authentication middleware didn't populate user but Authorization header present,
            // try to decode JWT and extract name identifier claim. This keeps the return workflow working
            // when header is forwarded but middleware not applied.
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    var handler = new JwtSecurityTokenHandler();
                    if (handler.CanReadToken(token))
                    {
                        var jwt = handler.ReadJwtToken(token);
                        var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "nameid" || c.Type == "id");
                        if (idClaim != null && Guid.TryParse(idClaim.Value, out var parsed))
                        {
                            return parsed;
                        }
                    }
                }
            }
            catch
            {
                // ignore and return null
            }

            return null;
        }

        private static string ResolveFinalAssetStatus(string? inspectionResult, string? handbackCondition)
        {
            var normalizedResult = (inspectionResult ?? "").Trim().ToUpperInvariant();
            if (normalizedResult == "AVAILABLE")
                return AssetStatus.AVAILABLE.ToString();

            if (normalizedResult == "BROKEN")
                return AssetStatus.BROKEN.ToString();

            if (normalizedResult == "MAINTENANCE" || normalizedResult == "UNDER_MAINTENANCE")
                return AssetStatus.UNDER_MAINTENANCE.ToString();

            var normalizedCondition = (handbackCondition ?? "").Trim().ToUpperInvariant();

            return normalizedCondition switch
            {
                "GOOD" => AssetStatus.AVAILABLE.ToString(),
                "BROKEN" => AssetStatus.BROKEN.ToString(),
                "FAIR" => AssetStatus.UNDER_MAINTENANCE.ToString(),
                "DAMAGED" => AssetStatus.UNDER_MAINTENANCE.ToString(),
                "MISSING_ACCESSORIES" => AssetStatus.UNDER_MAINTENANCE.ToString(),
                _ => AssetStatus.UNDER_MAINTENANCE.ToString()
            };
        }

        private static string MergeNotes(params string?[] values)
        {
            return string.Join("\n", values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim()));
        }
    }

    public class InitiateReturnRequestDto
    {
        public Guid AssignmentId { get; set; }
        public Guid AssetId { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public DateTime? ExpectedReturnDate { get; set; }
    }

    public class ReceiveReturnRequestDto
    {
        public DateTime? ReceivedAt { get; set; }
        public bool PhysicallyReceived { get; set; } = true;
        public string? HandbackCondition { get; set; }
        public string? Notes { get; set; }
    }

    public class InspectReturnRequestDto
    {
        public string? HandbackCondition { get; set; }
        public string? InspectionNotes { get; set; }
        public string? AccessoriesNotes { get; set; }
        public string? InspectionResult { get; set; }
    }
}