using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.interfaces
{
    public interface IAssetLifecycleRepository
    {
        Task<Asset?> GetAssetAggregateAsync(Guid assetId);
        Task<bool> HasActiveAssignmentAsync(Guid assetId);
        Task<bool> HasOpenWorkflowAsync(Guid assetId);
        Task<MaintenanceRecord?> GetLatestOpenMaintenanceAsync(Guid assetId);
        Task AddAssetHistoryAsync(AssetHistory history);
        Task AddLiquidationAsync(AssetLiquidation liquidation);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
        Task CloseActiveAssignmentAsync(Guid assetId, DateTime closedAt);

        Task CloseOpenBrokenReportsAsync(Guid assetId, DateTime closedAt);
        Task CloseOpenReturnRequestsAsync(Guid assetId, DateTime closedAt);
        Task CancelActiveReservationsAsync(Guid assetId, DateTime closedAt);

    }
}