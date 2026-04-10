namespace AssetManagement.Application.dtos
{
    public class MarkAssetBeyondRepairRequest
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class LiquidateAssetRequest
    {
        public string Reason { get; set; } = string.Empty;
        public DateOnly LiquidationDate { get; set; }
        public string? DisposalMethod { get; set; }
        public string? Notes { get; set; }
    }

    public class AssetLifecycleActionResponse
    {
        public Guid AssetId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsBeyondRepair { get; set; }
        public string? BeyondRepairReason { get; set; }
        public Guid? LiquidationId { get; set; }
        public DateTime ChangedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}