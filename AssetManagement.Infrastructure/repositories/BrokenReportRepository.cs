using AssetManagement.Application.interfaces;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class BrokenReportRepository : IBrokenReportRepository
    {
        private readonly AssetDbContext _context;

        public BrokenReportRepository(AssetDbContext context)
        {
            _context = context;
        }

        public async Task<List<BrokenReport>> GetAllAsync(string? search)
        {
            var query = _context.BrokenReports
                .Include(x => x.Asset)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.Asset.AssetTag.Contains(search) || x.Description.Contains(search));
            }

            return await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        }

        public async Task<BrokenReport?> GetByIdAsync(Guid id)
        {
            return await _context.BrokenReports.Include(x => x.Asset).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> AcceptReportAsync(Guid reportId, Guid adminId)
        {
            var report = await _context.BrokenReports.Include(x => x.Asset).FirstOrDefaultAsync(x => x.Id == reportId && !x.IsDeleted);
            if (report?.Asset == null) return false;
            if (string.Equals(report.Status, "ACCEPTED", StringComparison.OrdinalIgnoreCase)) return true;

            var activeAssignment = await _context.Assignments
                .Where(x => !x.IsDeleted && x.AssetId == report.AssetId && x.Status == "ASSIGNED" && x.ReturnedAt == null)
                .OrderByDescending(x => x.AssignedAt)
                .FirstOrDefaultAsync();

            var previousStatus = report.Asset.Status;
            var issueType = NormalizeIssueType(report.TriageResult);
            var now = DateTime.UtcNow;

            report.Status = "ACCEPTED";
            report.TriageAt = now;
            report.TriageById = adminId;

            if (issueType == "LOST")
            {
                report.Asset.Status = AssetStatus.LOST.ToString();
                report.Asset.CurrentUserId = activeAssignment?.UserId;
            }
            else
            {
                report.Asset.Status = AssetStatus.UNDER_MAINTENANCE.ToString();
            }

            report.Asset.UpdatedAt = now;

            _context.AssetHistories.Add(new AssetHistory
            {
                Id = Guid.NewGuid(),
                AssetId = report.AssetId,
                AssignmentId = activeAssignment?.Id,
                UserId = activeAssignment?.UserId,
                ChangedById = adminId,
                Action = issueType == "LOST" ? "APPROVED" : "APPROVED",
                PreviousStatus = previousStatus,
                NewStatus = report.Asset.Status,
                Note = issueType == "LOST" ? "Admin approved lost asset report" : "Admin approved broken asset report",
                ChangedAt = now,
                CreatedAt = now,
            });

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectReportAsync(Guid reportId, Guid adminId, string? reason)
        {
            var report = await _context.BrokenReports.Include(x => x.Asset).FirstOrDefaultAsync(x => x.Id == reportId && !x.IsDeleted);
            if (report?.Asset == null) return false;
            if (string.Equals(report.Status, "REJECTED", StringComparison.OrdinalIgnoreCase)) return true;

            var activeAssignment = await _context.Assignments
                .Where(x => !x.IsDeleted && x.AssetId == report.AssetId && x.Status == "ASSIGNED" && x.ReturnedAt == null)
                .OrderByDescending(x => x.AssignedAt)
                .FirstOrDefaultAsync();

            var now = DateTime.UtcNow;
            var previousStatus = report.Asset.Status;
            var issueType = NormalizeIssueType(report.TriageResult);

            report.Status = "REJECTED";
            report.TriageAt = now;
            report.TriageById = adminId;
            report.ClosureReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();

            if (string.Equals(previousStatus, AssetStatus.REPORTED_BROKEN.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                report.Asset.Status = activeAssignment == null ? AssetStatus.AVAILABLE.ToString() : AssetStatus.IN_USE.ToString();
                report.Asset.UpdatedAt = now;
            }

            _context.AssetHistories.Add(new AssetHistory
            {
                Id = Guid.NewGuid(),
                AssetId = report.AssetId,
                AssignmentId = activeAssignment?.Id,
                UserId = activeAssignment?.UserId,
                ChangedById = adminId,
                Action = issueType == "LOST" ? "REJECTED" : "REJECTED",
                PreviousStatus = previousStatus,
                NewStatus = report.Asset.Status,
                Note = report.ClosureReason ?? "Admin rejected issue report",
                ChangedAt = now,
                CreatedAt = now,
            });

            await _context.SaveChangesAsync();
            return true;
        }

        private static string NormalizeIssueType(string? value)
        {
            var normalized = (value ?? "BROKEN").Trim().ToUpperInvariant();
            return normalized == "LOST" ? "LOST" : "BROKEN";
        }
    }
}