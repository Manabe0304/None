using AssetManagement.Domain.Constants;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/broken-reports")]
    [Authorize]
    public class BrokenReportController : ControllerBase
    {
        private readonly AssetDbContext _context;

        public BrokenReportController(AssetDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "EMPLOYEE")]
        public async Task<IActionResult> Create([FromBody] CreateBrokenReportRequest request)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdStr)) return Unauthorized("No user");
                if (!Guid.TryParse(userIdStr, out var userId)) return BadRequest("Invalid user");
                if (request.AssetId == Guid.Empty) return BadRequest(new { message = "AssetId is required." });
                if (string.IsNullOrWhiteSpace(request.Description)) return BadRequest(new { message = "Description is required." });

                var issueType = NormalizeIssueType(request.IssueType);

                var assignment = await _context.Assignments
                    .Include(x => x.Asset)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.AssetId == request.AssetId && x.UserId == userId && x.Status == "ASSIGNED" && x.ReturnedAt == null);

                if (assignment == null) return BadRequest(new { message = "Asset is not currently assigned to this employee." });

                if (assignment.Asset != null &&
                    (string.Equals(assignment.Asset.Status, AssetStatus.BEYOND_REPAIR.ToString(), StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(assignment.Asset.Status, AssetStatus.LIQUIDATED.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { message = "Cannot report a BEYOND_REPAIR or LIQUIDATED asset." });
                }

                var hasOpenReport = await _context.BrokenReports.AnyAsync(x =>
                    !x.IsDeleted && x.AssetId == request.AssetId && (x.Status == "OPEN" || x.Status == "ACCEPTED"));
                if (hasOpenReport) return BadRequest(new { message = "This asset already has an open issue report." });

                var asset = assignment.Asset;
                var previousStatus = asset.Status;
                var now = DateTime.UtcNow;

                var report = new BrokenReport
                {
                    Id = Guid.NewGuid(),
                    AssetId = request.AssetId,
                    ReportedById = userId,
                    Description = request.Description.Trim(),
                    Status = "OPEN",
                    TriageResult = issueType,
                    CreatedAt = now,
                    UpdatedAt = now,
                };

                _context.BrokenReports.Add(report);

                if (issueType == "BROKEN")
                {
                    asset.Status = AssetStatus.REPORTED_BROKEN.ToString();
                    asset.UpdatedAt = now;
                }

                _context.AssetHistories.Add(new AssetHistory
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    AssignmentId = assignment.Id,
                    UserId = assignment.UserId,
                    ChangedById = userId,
                    Action = issueType == "LOST" ? "REPORTED_LOST" : AssetHistoryActions.ReportedBroken,
                    PreviousStatus = previousStatus,
                    NewStatus = asset.Status,
                    Note = report.Description,
                    ChangedAt = now,
                    CreatedAt = now,
                });

                await _context.SaveChangesAsync();
                return Ok(new { message = issueType == "LOST" ? "Lost asset report created successfully" : "Report created successfully", status = asset.Status, issueType });
            }
            catch (Exception ex)
            {
                var innerError = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { error = innerError });
            }
        }

        private static string NormalizeIssueType(string? value)
        {
            var normalized = (value ?? "BROKEN").Trim().ToUpperInvariant();
            return normalized == "LOST" ? "LOST" : "BROKEN";
        }

        public class CreateBrokenReportRequest
        {
            public Guid AssetId { get; set; }
            public string Description { get; set; } = string.Empty;
            public string IssueType { get; set; } = "BROKEN";
        }
    }
}