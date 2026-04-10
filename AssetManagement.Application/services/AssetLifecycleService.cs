using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.services
{
    public class AssetLifecycleService : IAssetLifecycleService
    {
        private readonly IAssetLifecycleRepository _repo;

        public AssetLifecycleService(IAssetLifecycleRepository repo)
        {
            _repo = repo;
        }

        public Task<AssetLifecycleActionResponse> MarkBeyondRepairAsync(Guid assetId, MarkAssetBeyondRepairRequest request, Guid adminId)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ArgumentException("Reason is required.");

            return _repo.ExecuteInTransactionAsync(async () =>
            {
                var asset = await _repo.GetAssetAggregateAsync(assetId)
                    ?? throw new KeyNotFoundException("Asset not found.");

                if (string.Equals(asset.Status, AssetStatus.LIQUIDATED.ToString(), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Liquidated asset cannot be marked as beyond repair.");

                if (!string.Equals(asset.Status, AssetStatus.UNDER_MAINTENANCE.ToString(), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Only assets in UNDER_MAINTENANCE can be marked as beyond repair.");

                var now = DateTime.UtcNow;
                var previousStatus = asset.Status;

                asset.Status = AssetStatus.BEYOND_REPAIR.ToString();
                asset.IsBeyondRepair = true;
                asset.BeyondRepairReason = request.Reason.Trim();
                asset.UpdatedAt = now;

                var latestMaintenance = await _repo.GetLatestOpenMaintenanceAsync(asset.Id);
                if (latestMaintenance != null)
                {
                    latestMaintenance.Outcome = "BEYOND_REPAIR";
                    latestMaintenance.Status = "CLOSED";
                    latestMaintenance.CompletedAt ??= now;
                    latestMaintenance.UpdatedAt = now;
                }

                await _repo.AddAssetHistoryAsync(new AssetHistory
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    ChangedById = adminId,
                    Action = AssetHistoryActions.MarkBeyondRepair,
                    PreviousStatus = previousStatus,
                    NewStatus = asset.Status,
                    Note = request.Reason.Trim(),
                    ChangedAt = now,
                    CreatedAt = now
                });

                return new AssetLifecycleActionResponse
                {
                    AssetId = asset.Id,
                    Status = asset.Status,
                    IsBeyondRepair = asset.IsBeyondRepair,
                    BeyondRepairReason = asset.BeyondRepairReason,
                    ChangedAt = now,
                    Message = "Asset marked as beyond repair successfully."
                };
            });
        }

        public Task<AssetLifecycleActionResponse> LiquidateAsync(Guid assetId, LiquidateAssetRequest request, Guid adminId)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new ArgumentException("Reason is required.");

            if (request.LiquidationDate == default)
                throw new ArgumentException("Liquidation date is required.");

            return _repo.ExecuteInTransactionAsync(async () =>
            {
                var asset = await _repo.GetAssetAggregateAsync(assetId)
                    ?? throw new KeyNotFoundException("Asset not found.");

                if (string.Equals(asset.Status, AssetStatus.LIQUIDATED.ToString(), StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Asset is already liquidated.");

                var isBeyondRepair =
                    string.Equals(asset.Status, AssetStatus.BEYOND_REPAIR.ToString(), StringComparison.OrdinalIgnoreCase) ||
                    asset.IsBeyondRepair;

                if (!isBeyondRepair)
                    throw new InvalidOperationException("Asset must be marked BEYOND_REPAIR before liquidation.");

                if (await _repo.HasActiveAssignmentAsync(asset.Id))
                    throw new InvalidOperationException("Asset still has an active assignment. Resolve it first.");

                if (await _repo.HasOpenWorkflowAsync(asset.Id))
                    throw new InvalidOperationException("Asset still has unfinished workflow. Close it before liquidation.");

                var now = DateTime.UtcNow;
                var previousStatus = asset.Status;

                var liquidation = new AssetLiquidation
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    LiquidationReason = request.Reason.Trim(),
                    LiquidationDate = request.LiquidationDate,
                    DisposalMethod = request.DisposalMethod?.Trim(),
                    LiquidatedById = adminId,
                    Notes = request.Notes?.Trim(),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                asset.Status = AssetStatus.LIQUIDATED.ToString();
                asset.UpdatedAt = now;

                await _repo.AddLiquidationAsync(liquidation);

                await _repo.AddAssetHistoryAsync(new AssetHistory
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    ChangedById = adminId,
                    Action = AssetHistoryActions.Liquidated,
                    PreviousStatus = previousStatus,
                    NewStatus = asset.Status,
                    Note = BuildLiquidationNote(request),
                    ChangedAt = now,
                    CreatedAt = now
                });

                return new AssetLifecycleActionResponse
                {
                    AssetId = asset.Id,
                    Status = asset.Status,
                    IsBeyondRepair = asset.IsBeyondRepair,
                    BeyondRepairReason = asset.BeyondRepairReason,
                    LiquidationId = liquidation.Id,
                    ChangedAt = now,
                    Message = "Asset liquidated successfully."
                };
            });
        }

        private static string BuildLiquidationNote(LiquidateAssetRequest request)
        {
            return
                $"Reason: {request.Reason.Trim()}\n" +
                $"Liquidation Date: {request.LiquidationDate:yyyy-MM-dd}\n" +
                $"Disposal Method: {(string.IsNullOrWhiteSpace(request.DisposalMethod) ? "N/A" : request.DisposalMethod.Trim())}\n" +
                $"Notes: {(string.IsNullOrWhiteSpace(request.Notes) ? "N/A" : request.Notes.Trim())}";
        }
    }
}