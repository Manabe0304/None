namespace AssetManagement.Application.dtos
{
    public class AssetCreateUpdateDto
    {
        public string AssetTag { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseValue { get; set; }
        public string Status { get; set; } = "AVAILABLE";
        public Guid? DepartmentId { get; set; }
        public Guid? CurrentUserId { get; set; }
        public string AssignmentType { get; set; } = "UNASSIGNED";
    }
}