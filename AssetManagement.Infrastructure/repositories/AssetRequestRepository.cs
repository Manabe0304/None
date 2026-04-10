using AssetManagement.Application.interfaces;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class AssetRequestRepository : IAssetRequestRepository
    {
        private readonly AssetDbContext _context;

        public AssetRequestRepository(AssetDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetEmployeeForRequestAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                    .ThenInclude(d => d!.Manager)
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    !u.IsDeleted &&
                    u.Status == "ACTIVE");
        }

        public async Task AddAssetRequestAsync(AssetRequest request)
        {
            await _context.AssetRequests.AddAsync(request);
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public async Task<List<AssetRequest>> GetMyRequestsAsync(Guid employeeId)
        {
            return await _context.AssetRequests
                .Include(r => r.Assignments)
                    .ThenInclude(a => a.Asset)
                .Where(r => r.EmployeeId == employeeId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetManagerWithDepartmentAsync(Guid managerId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == managerId && !u.IsDeleted);
        }

        public async Task<List<AssetRequest>> GetPendingRequestsByDepartmentAsync(Guid departmentId)
        {
            return await _context.AssetRequests
                .Include(r => r.Employee)
                .Where(r =>
                    !r.IsDeleted &&
                    r.Status == AssetRequestStatuses.Pending &&
                    r.Employee != null &&
                    !r.Employee.IsDeleted &&
                    r.Employee.DepartmentId == departmentId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<AssetRequest?> GetRequestForManagerDecisionAsync(Guid requestId, Guid managerDepartmentId)
        {
            return await _context.AssetRequests
                .Include(r => r.Employee)
                .FirstOrDefaultAsync(r =>
                    r.Id == requestId &&
                    !r.IsDeleted &&
                    r.Status == AssetRequestStatuses.Pending &&
                    r.Employee != null &&
                    !r.Employee.IsDeleted &&
                    r.Employee.DepartmentId == managerDepartmentId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}