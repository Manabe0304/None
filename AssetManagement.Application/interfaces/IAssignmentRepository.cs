using AssetManagement.Application.dtos;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.interfaces
{
    public interface IAssignmentRepository
    {
        Task<List<AvailableAssetDto>> GetAvailableAssetsAsync();
        Task<List<ApprovedRequestDto>> GetApprovedRequestsAsync();

        Task<AssetRequest?> GetRequestForAssignmentAsync(Guid requestId);
        Task<Asset?> GetAssetByIdAsync(Guid assetId);
        Task<bool> HasOpenAssignmentAsync(Guid assetId, string assignedStatus);

        Task AddAssignmentAsync(Assignment assignment);
        Task AddAssetHistoryAsync(AssetHistory history);
        Task AddAuditLogAsync(AuditLog auditLog);
        Task AddNotificationAsync(Notification notification);

        Task<List<CurrentAssignmentDto>> GetCurrentAssignmentsAsync(string? employee = null, string? department = null, string? assetType = null);

        Task<List<AssetHistoryItemDto>> GetAssetHistoryAsync(
            string? assetKeyword = null,
            string? employee = null,
            string? department = null,
            string? assetType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        Task<List<MyAssignmentDto>> GetAssignmentsByUserIdAsync(Guid userId);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
    }
}
