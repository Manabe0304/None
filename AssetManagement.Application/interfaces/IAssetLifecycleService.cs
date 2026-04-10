using AssetManagement.Application.dtos;

namespace AssetManagement.Application.interfaces
{
    public interface IAssetLifecycleService
    {
        Task<AssetLifecycleActionResponse> MarkBeyondRepairAsync(Guid assetId, MarkAssetBeyondRepairRequest request, Guid adminId);
        Task<AssetLifecycleActionResponse> LiquidateAsync(Guid assetId, LiquidateAssetRequest request, Guid adminId);
    }
}