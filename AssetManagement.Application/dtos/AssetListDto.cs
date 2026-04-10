namespace AssetManagement.Application.dtos
{
    public class AssetListDto
    {
        public Guid Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseValue { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? CurrentUserId { get; set; }
        public string? CurrentUserName { get; set; }
        public string AssignmentType { get; set; } = "UNASSIGNED";
    }
}