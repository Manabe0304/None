using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/assets")]
    [Authorize]
    public class AssetDetailsController : ControllerBase
    {
        private readonly AssetDbContext _context;

        public AssetDetailsController(AssetDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var asset = await _context.Assets
                .AsNoTracking()
                .Include(a => a.Department)
                .Include(a => a.CurrentUser)
                .Include(a => a.Liquidation)
                    .ThenInclude(l => l.LiquidatedBy)
                .Include(a => a.Histories)
                    .ThenInclude(h => h.ChangedBy)
                .Include(a => a.Histories)
                    .ThenInclude(h => h.User)
                        .ThenInclude(u => u.Department)
                .Include(a => a.Assignments)
                    .ThenInclude(x => x.Request)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            if (asset == null) return NotFound(new { message = "Asset not found." });

            var requestTimeline = asset.Assignments
                .Where(x => x.Request != null)
                .Select(x => new
                {
                    source = "REQUEST",
                    action = "REQUEST_CREATED",
                    changedAt = x.Request!.CreatedAt,
                    previousStatus = (string?)null,
                    newStatus = x.Request.Status.ToString(),
                    changedBy = x.Request.EmployeeId.ToString(),
                    note = $"{x.Request.AssetType} / {x.Request.PreferredModel ?? "N/A"} / {x.Request.Reason}"
                });

            var historyTimeline = asset.Histories.Select(h => new
            {
                source = "HISTORY",
                action = h.Action,
                changedAt = h.ChangedAt,
                previousStatus = h.PreviousStatus,
                newStatus = h.NewStatus,
                changedBy = h.ChangedBy != null ? (h.ChangedBy.FullName ?? h.ChangedBy.Email) : null,
                note = h.Note
            });

            var timeline = requestTimeline
                .Concat(historyTimeline)
                .OrderByDescending(x => x.changedAt)
                .ToList();

            return Ok(new
            {
                data = new
                {
                    id = asset.Id,
                    assetTag = asset.AssetTag,
                    category = asset.Category,
                    model = asset.Model,
                    serialNumber = asset.SerialNumber,
                    status = asset.Status,
                    department = asset.Department != null ? asset.Department.Name : null,
                    currentUser = asset.CurrentUser != null ? (asset.CurrentUser.FullName ?? asset.CurrentUser.Email) : null,
                    isBeyondRepair = asset.IsBeyondRepair,
                    beyondRepairReason = asset.BeyondRepairReason,
                    liquidation = asset.Liquidation == null ? null : new
                    {
                        id = asset.Liquidation.Id,
                        reason = asset.Liquidation.LiquidationReason,
                        liquidationDate = asset.Liquidation.LiquidationDate,
                        disposalMethod = asset.Liquidation.DisposalMethod,
                        notes = asset.Liquidation.Notes,
                        createdBy = asset.Liquidation.LiquidatedBy != null
                            ? (asset.Liquidation.LiquidatedBy.FullName ?? asset.Liquidation.LiquidatedBy.Email)
                            : null
                    },
                    history = timeline
                }
            });
        }
    }
}