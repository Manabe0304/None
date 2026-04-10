namespace AssetManagement.Application.dtos.Response.AssetRequests
{
    public class AssetRequestResponse
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? ManagerId { get; set; }
        public string AssetType { get; set; } = string.Empty;
        public string? PreferredModel { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
