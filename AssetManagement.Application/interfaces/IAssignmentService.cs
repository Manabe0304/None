using AssetManagement.Application.dtos;

namespace AssetManagement.Application.interfaces
{
    public interface IAssignmentService
    {
        Task<List<MyAssignmentDto>> GetAssignmentsByUserIdAsync(Guid userId);
        Task<List<AvailableAssetDto>> GetAvailableAssetsAsync();
        Task<List<ApprovedRequestDto>> GetApprovedRequestsAsync();
        Task<AssignmentResultDto> AssignAsync(AssignmentCreateDto dto, Guid adminId);
        Task<List<CurrentAssignmentDto>> GetCurrentAssignmentsAsync(string? employee = null, string? department = null, string? assetType = null);
        Task<List<AssetHistoryItemDto>> GetAssetHistoryAsync(
            string? assetKeyword = null,
            string? employee = null,
            string? department = null,
            string? assetType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
    }
}
