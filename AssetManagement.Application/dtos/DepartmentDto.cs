namespace AssetManagement.Application.dtos
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public Guid? ManagerId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsDeleted { get; set; }
        public int EmployeeCount { get; set; }
        public int TotalAssetCount { get; set; }
        public int SharedAssetCount { get; set; }
        public int UnassignedUserCountOnDelete { get; set; }
        public List<DepartmentEmployeeAssetDto> Employees { get; set; } = [];
        public List<DepartmentSharedAssetDto> SharedAssets { get; set; } = [];
    }

    public class DepartmentEmployeeAssetDto
    {
        public Guid UserId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int AssetCount { get; set; }
    }

    public class DepartmentSharedAssetDto
    {
        public Guid AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string? AssetName { get; set; }
        public string? Status { get; set; }
    }
}