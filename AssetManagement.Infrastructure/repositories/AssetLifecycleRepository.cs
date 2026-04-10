using AssetManagement.Application.interfaces;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class AssetLifecycleRepository : IAssetLifecycleRepository
    {
        private readonly AssetDbContext _context;

        public AssetLifecycleRepository(AssetDbContext context)
        {
            _context = context;
        }

        public Task<Asset?> GetAssetAggregateAsync(Guid assetId)
        {
            return _context.Assets
                .Include(a => a.Liquidation)
                .FirstOrDefaultAsync(a => a.Id == assetId && !a.IsDeleted);
        }

        public Task<bool> HasActiveAssignmentAsync(Guid assetId)
        {
            return _context.Assignments.AnyAsync(a =>
                !a.IsDeleted &&
                a.AssetId == assetId &&
                a.Status == "ASSIGNED" &&
                a.ReturnedAt == null);
        }

        public async Task<bool> HasOpenWorkflowAsync(Guid assetId)
        {
            var hasOpenReturn = await _context.ReturnRequests.AnyAsync(r =>
                !r.IsDeleted &&
                r.AssetId == assetId &&
                r.Status != "INSPECTED");

            var hasOpenBrokenReport = await _context.BrokenReports.AnyAsync(r =>
                !r.IsDeleted &&
                r.AssetId == assetId &&
                (r.Status == "OPEN" || r.Status == "ACCEPTED"));

            var hasOpenReservation = await _context.AssetReservations.AnyAsync(r =>
                r.AssetId == assetId &&
                r.Status == "ACTIVE");

            var hasOpenMaintenance = await _context.MaintenanceRecords.AnyAsync(m =>
                !m.IsDeleted &&
                m.AssetId == assetId &&
                (m.CompletedAt == null || m.Status == "UNDER_MAINTENANCE" || m.Outcome == "PENDING"));

            return hasOpenReturn || hasOpenBrokenReport || hasOpenReservation || hasOpenMaintenance;
        }

        public Task<MaintenanceRecord?> GetLatestOpenMaintenanceAsync(Guid assetId)
        {
            return _context.MaintenanceRecords
                .Where(m => !m.IsDeleted && m.AssetId == assetId)
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync(m => m.CompletedAt == null || m.Status == "UNDER_MAINTENANCE");
        }

        public Task AddAssetHistoryAsync(AssetHistory history)
        {
            _context.AssetHistories.Add(history);
            return Task.CompletedTask;
        }

        public Task AddLiquidationAsync(AssetLiquidation liquidation)
        {
            _context.Liquidations.Add(liquidation);
            return Task.CompletedTask;
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return result;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}