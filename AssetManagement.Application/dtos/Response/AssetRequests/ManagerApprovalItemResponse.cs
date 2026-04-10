namespace AssetManagement.Application.dtos.Response.AssetRequests
{
    public class ManagerApprovalItemResponse
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Urgency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
    }
}