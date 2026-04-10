using AssetManagement.Application.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/admin/broken-reports")]
    [Authorize(Roles = "ADMIN")]
    public class AdminBrokenReportController : ControllerBase
    {
        private readonly IBrokenReportRepository _repo;

        public AdminBrokenReportController(IBrokenReportRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var data = await _repo.GetAllAsync(search);
            return Ok(new
            {
                data = data.Select(x => new
                {
                    x.Id,
                    x.Description,
                    x.Status,
                    x.CreatedAt,
                    x.ClosureReason,
                    issueType = string.Equals(x.TriageResult, "LOST", StringComparison.OrdinalIgnoreCase) ? "LOST" : "BROKEN",
                    assetTag = x.Asset.AssetTag,
                    assetId = x.AssetId,
                }),
            });
        }

        [HttpPost("{id}/accept")]
        public async Task<IActionResult> Accept(Guid id)
        {
            var rawAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(rawAdminId, out var adminId)) return Unauthorized(new { message = "Invalid token." });
            var result = await _repo.AcceptReportAsync(id, adminId);
            if (!result) return NotFound("Report not found");
            return Ok(new { message = "Accepted successfully" });
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectIssueReportRequest? request)
        {
            var rawAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(rawAdminId, out var adminId)) return Unauthorized(new { message = "Invalid token." });
            var result = await _repo.RejectReportAsync(id, adminId, request?.Reason);
            if (!result) return NotFound("Report not found");
            return Ok(new { message = "Rejected successfully" });
        }
    }

    public class RejectIssueReportRequest
    {
        public string? Reason { get; set; }
    }
}