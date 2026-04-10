namespace AssetManagement.Application.dtos.Response.ReturnRequests
{
    public class ReturnedAssetHistoryDetailResponse
    {
        public Guid Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string AssetType { get; set; } = string.Empty;
        public DateTime ReturnInitiatedDate { get; set; }
        public string? ReturnReason { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ProcessedDate { get; set; }

        public string? HandlingNotes { get; set; }
        public string? InitialReceptionResult { get; set; }
        public string? HandledByName { get; set; }

        public Guid AssignmentId { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
    }
}