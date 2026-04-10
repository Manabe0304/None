namespace AssetManagement.Application.dtos.Response.AssetRequests
{
    public class MyAssetRequestItemResponse
    {
        public Guid Id { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestedDeviceType { get; set; } = string.Empty;
        public string? PreferredModel { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public AssignmentInfoResponse? Assignment { get; set; }
    }

    public class AssignmentInfoResponse
    {
        public Guid AssignmentId { get; set; }
        public Guid AssetId { get; set; }
        public string? AssetTag { get; set; }
        public string? AssetCategory { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}