using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly AssetDbContext _context;

        public AssignmentRepository(AssetDbContext context)
        {
            _context = context;
        }

        public async Task<List<MyAssignmentDto>> GetAssignmentsByUserIdAsync(Guid userId)
        {
            return await _context.Assignments
                .Include(a => a.Asset)
                .Where(a => a.UserId == userId && a.Status == "ASSIGNED" && !a.IsDeleted)
                .OrderByDescending(a => a.AssignedAt)
                .Select(a => new MyAssignmentDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    AssetTag = a.Asset.AssetTag,
                    AssetCategory = a.Asset.Category,
                    AssignedAt = a.AssignedAt,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<List<AvailableAssetDto>> GetAvailableAssetsAsync()
        {
            return await _context.Assets
                .AsNoTracking()
                .Where(a => !a.IsDeleted && a.Status == "AVAILABLE")
                .OrderBy(a => a.AssetTag)
                .Select(a => new AvailableAssetDto
                {
                    Id = a.Id,
                    AssetTag = a.AssetTag,
                    AssetName = a.Model ?? a.AssetTag,
                    AssetType = a.Category,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<List<ApprovedRequestDto>> GetApprovedRequestsAsync()
        {
            var allowedStatuses = new[] { AssetRequestStatuses.Approved, AssetRequestStatuses.PendingAdminAssignment };

            return await _context.AssetRequests
                .AsNoTracking()
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Manager)
                .Where(r => !r.IsDeleted && allowedStatuses.Contains(r.Status))
                .OrderByDescending(r => r.DecisionAt ?? r.CreatedAt)
                .Select(r => new ApprovedRequestDto
                {
                    Id = r.Id,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee.FullName ?? r.Employee.Email,
                    DepartmentName = r.Employee.Department != null ? r.Employee.Department.Name : null,
                    AssetType = r.AssetType,
                    PreferredModel = r.PreferredModel,
                    Reason = r.Reason,
                    Urgency = r.UrgencyLevel,
                    ApprovedByManager = r.Manager != null ? r.Manager.FullName : null,
                    ApprovedAt = r.DecisionAt,
                    Status = r.Status
                })
                .ToListAsync();
        }

        public Task<AssetRequest?> GetRequestForAssignmentAsync(Guid requestId)
        {
            return _context.AssetRequests
                .Include(r => r.Employee)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted);
        }

        public Task<Asset?> GetAssetByIdAsync(Guid assetId)
        {
            return _context.Assets.FirstOrDefaultAsync(a => a.Id == assetId);
        }

        public Task<bool> HasOpenAssignmentAsync(Guid assetId, string assignedStatus)
        {
            return _context.Assignments.AnyAsync(a =>
                !a.IsDeleted
                && a.AssetId == assetId
                && a.Status == assignedStatus
                && a.ReturnedAt == null);
        }

        public Task AddAssignmentAsync(Assignment assignment)
        {
            _context.Assignments.Add(assignment);
            return Task.CompletedTask;
        }

        public Task AddAssetHistoryAsync(AssetHistory history)
        {
            _context.AssetHistories.Add(history);
            return Task.CompletedTask;
        }

        public Task AddAuditLogAsync(AuditLog auditLog)
        {
            _context.AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }

        public Task AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public async Task<List<CurrentAssignmentDto>> GetCurrentAssignmentsAsync(string? employee = null, string? department = null, string? assetType = null)
        {
            var query = _context.Assignments
                .AsNoTracking()
                .Include(a => a.Asset)
                .Include(a => a.User)
                    .ThenInclude(u => u.Department)
                .Where(a => !a.IsDeleted && a.Status == "ASSIGNED");

            if (!string.IsNullOrWhiteSpace(employee))
            {
                var keyword = employee.Trim();
                query = query.Where(a =>
                    (a.User.FullName ?? a.User.Email).Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                var keyword = department.Trim();
                query = query.Where(a =>
                    a.User.Department != null &&
                    a.User.Department.Name.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(assetType))
            {
                var keyword = assetType.Trim();
                query = query.Where(a =>
                    a.Asset.Category != null &&
                    a.Asset.Category.Contains(keyword));
            }

            return await query
                .OrderByDescending(a => a.AssignedAt)
                .Select(a => new CurrentAssignmentDto
                {
                    Id = a.Id,
                    AssetId = a.AssetId,
                    EmployeeId = a.UserId,
                    AssetTag = a.Asset.AssetTag,
                    AssetName = a.Asset.Model ?? a.Asset.AssetTag,
                    Category = a.Asset.Category,
                    EmployeeName = a.User.FullName ?? a.User.Email,
                    Department = a.User.Department != null ? a.User.Department.Name : "N/A",
                    AssignedAt = a.AssignedAt,
                    Status = a.Status
                })
                .ToListAsync();
        }

        public async Task<List<AssetHistoryItemDto>> GetAssetHistoryAsync(
            string? assetKeyword = null,
            string? employee = null,
            string? department = null,
            string? assetType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.AssetHistories
                .AsNoTracking()
                .Include(h => h.Asset)
                .Include(h => h.User)
                    .ThenInclude(u => u.Department)
                .Include(h => h.ChangedBy)
                .Where(h => h.Asset != null);

            if (!string.IsNullOrWhiteSpace(assetKeyword))
            {
                var keyword = assetKeyword.Trim();
                query = query.Where(h =>
                    h.Asset.AssetTag.Contains(keyword) ||
                    (h.Asset.Model != null && h.Asset.Model.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(employee))
            {
                var keyword = employee.Trim();
                query = query.Where(h =>
                    h.User != null &&
                    ((h.User.FullName ?? h.User.Email).Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                var keyword = department.Trim();
                query = query.Where(h =>
                    h.User != null &&
                    h.User.Department != null &&
                    h.User.Department.Name.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(assetType))
            {
                var keyword = assetType.Trim();
                query = query.Where(h =>
                    h.Asset.Category != null &&
                    h.Asset.Category.Contains(keyword));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(h => h.ChangedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                var inclusiveEnd = toDate.Value.Date.AddDays(1);
                query = query.Where(h => h.ChangedAt < inclusiveEnd);
            }

            return await query
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new AssetHistoryItemDto
                {
                    Id = h.Id,
                    AssetId = h.AssetId,
                    AssignmentId = h.AssignmentId,
                    AssetTag = h.Asset.AssetTag,
                    AssetName = h.Asset.Model ?? h.Asset.AssetTag,
                    AssetType = h.Asset.Category,
                    Employee = h.User != null ? (h.User.FullName ?? h.User.Email) : null,
                    Department = h.User != null && h.User.Department != null ? h.User.Department.Name : null,
                    Action = h.Action,
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    ChangedBy = h.ChangedBy != null ? (h.ChangedBy.FullName ?? h.ChangedBy.Email) : null,
                    ChangedAt = h.ChangedAt,
                    Note = h.Note
                })
                .ToListAsync();
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var result = await action();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
