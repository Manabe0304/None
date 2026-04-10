using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/assignments")]
    public class AssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;
        private readonly AssetManagement.Infrastructure.Persistence.AssetDbContext _context;

        public AssignmentController(IAssignmentService assignmentService, AssetManagement.Infrastructure.Persistence.AssetDbContext context)
        {
            _assignmentService = assignmentService;
            _context = context;
        }

        [HttpGet("my")]
        [Authorize(Roles = "EMPLOYEE")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var userId = ResolveCurrentUserId();
            if (userId == null)
            {
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }

            // Sử dụng Service thay vì DBContext trực tiếp
            var assignments = await _assignmentService.GetAssignmentsByUserIdAsync(userId.Value);
            return Ok(assignments);
        }

        [HttpGet("available-assets")]
        public async Task<IActionResult> GetAvailableAssets()
        {
            var assets = await _assignmentService.GetAvailableAssetsAsync();
            return Ok(assets);
        }

        [HttpGet("approved-requests")]
        public async Task<IActionResult> GetApprovedRequests()
        {
            var requests = await _assignmentService.GetApprovedRequestsAsync();
            return Ok(requests);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Assign([FromBody] AssignmentCreateDto dto)
        {
            try
            {
                var adminId = ResolveCurrentUserId() ?? dto.AssignedById;

                if (!adminId.HasValue || adminId == Guid.Empty)
                {
                    return BadRequest(new { message = "Không xác định được người thực hiện assignment." });
                }

                var result = await _assignmentService.AssignAsync(dto, adminId.Value);
                return Ok(new { message = result.Message, assignmentId = result.AssignmentId });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent(
            [FromQuery] string? employee,
            [FromQuery] string? department,
            [FromQuery] string? assetType)
        {
            var assignments = await _assignmentService.GetCurrentAssignmentsAsync(employee, department, assetType);
            return Ok(assignments);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] string? asset,
            [FromQuery] string? employee,
            [FromQuery] string? department,
            [FromQuery] string? assetType,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate)
        {
            var history = await _assignmentService.GetAssetHistoryAsync(
                asset,
                employee,
                department,
                assetType,
                fromDate,
                toDate);

            return Ok(history);
        }

        private Guid? ResolveCurrentUserId()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}