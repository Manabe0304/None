namespace AssetManagement.Application.dtos.Response.ReturnRequests
{
    public class ReturnedAssetHistoryItemResponse
    {
        public Guid Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public DateTime ReturnInitiatedDate { get; set; }
        public string? ReturnReason { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedDate { get; set; }
    }
}