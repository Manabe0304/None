using AssetManagement.Application.interfaces.IRepository;
using AssetManagement.Domain.entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class ReturnRequestRepository : IReturnRequestRepository
    {
        private readonly AssetDbContext _context;

        public ReturnRequestRepository(AssetDbContext context)
        {
            _context = context;
        }

        public async Task<List<AssetReturnRequest>> GetMyReturnedHistoryAsync(Guid employeeId)
        {
            return await _context.Set<AssetReturnRequest>()
                .Include(r => r.Asset)
                .Include(r => r.Assignment)
                .Include(r => r.HandledBy)
                .Include(r => r.InitiatedBy)
                .Where(r => !r.IsDeleted
                            && r.UserId == employeeId
                            && r.Assignment.UserId == employeeId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<AssetReturnRequest?> GetMyReturnedHistoryDetailAsync(Guid employeeId, Guid returnRequestId)
        {
            return await _context.Set<AssetReturnRequest>()
                .Include(r => r.Asset)
                .Include(r => r.Assignment)
                .Include(r => r.HandledBy)
                .Include(r => r.InitiatedBy)
                .FirstOrDefaultAsync(r =>
                    !r.IsDeleted &&
                    r.Id == returnRequestId &&
                    r.UserId == employeeId &&
                    r.Assignment.UserId == employeeId);
        }
    }
}