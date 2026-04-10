namespace AssetManagement.Application.dtos
{
    public class AssignmentCreateDto
    {
        public Guid AssetId { get; set; }
        public Guid UserId { get; set; }
        public Guid? RequestId { get; set; }
        public Guid? AssignedById { get; set; }
        public string? Note { get; set; }
    }

    public class AssignmentResultDto
    {
        public Guid AssignmentId { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AvailableAssetDto
    {
        public Guid Id { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string? AssetName { get; set; }
        public string? AssetType { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ApprovedRequestDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }
        public string? AssetType { get; set; }
        public string? PreferredModel { get; set; }
        public string? Reason { get; set; }
        public string? Urgency { get; set; }
        public string? ApprovedByManager { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CurrentAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Guid EmployeeId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string? AssetName { get; set; }
        public string? Category { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public DateTime AssignedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AssetHistoryItemDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public Guid? AssignmentId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string? AssetName { get; set; }
        public string? AssetType { get; set; }
        public string? Employee { get; set; }
        public string? Department { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? PreviousStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Note { get; set; }
    }

    public class MyAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid AssetId { get; set; }
        public string AssetTag { get; set; } = string.Empty;
        public string? AssetCategory { get; set; }
        public DateTime AssignedAt { get; set; }
        // Bạn có thể thêm Status nếu muốn hiển thị cho người dùng
        public string Status { get; set; } = string.Empty;
    }
}
