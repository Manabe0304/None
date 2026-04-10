using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/my-assets")]
    [Authorize(Roles = "EMPLOYEE")]
    public class MyAssetsController : ControllerBase
    {
        private readonly AssetDbContext _context;

        public MyAssetsController(AssetDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAssets()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var assets = await _context.Assignments
                .AsNoTracking()
                .Include(a => a.Asset)
                    .ThenInclude(a => a.BrokenReports)
                .Include(a => a.Asset)
                    .ThenInclude(a => a.ReturnRequests)
                .Where(a =>
                    !a.IsDeleted &&
                    a.UserId == userId &&
                    a.Status == "ASSIGNED" &&
                    a.ReturnedAt == null)
                .OrderByDescending(a => a.AssignedAt)
                .Select(a => new
                {
                    assignmentId = a.Id,
                    assetId = a.AssetId,
                    assetTag = a.Asset.AssetTag,
                    category = a.Asset.Category,
                    model = a.Asset.Model,
                    serialNumber = a.Asset.SerialNumber,
                    assignedAt = a.AssignedAt,
                    status = a.Asset.Status,
                    assignmentStatus = a.Status,
                    hasOpenBrokenReport = a.Asset.BrokenReports.Any(r =>
                        !r.IsDeleted && (r.Status == "OPEN" || r.Status == "ACCEPTED")),
                    openReturnRequestStatus = a.Asset.ReturnRequests
                        .Where(r =>
                            !r.IsDeleted &&
                            r.AssignmentId == a.Id &&
                            r.UserId == userId &&
                            r.Status != "INSPECTED")
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => r.Status)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { data = assets });
        }
    }
}   