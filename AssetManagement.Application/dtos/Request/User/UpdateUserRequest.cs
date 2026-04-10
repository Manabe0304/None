namespace AssetManagement.Application.dtos.Request.User
{
    public class UpdateUserRequest
    {
        // Email optional when updating
        public string? Email { get; set; } = null;

        // Full name optional
        public string? FullName { get; set; } = null;

        // Password optional (only update if provided)
        public string? Password { get; set; } = null;

        // Either RoleId or RoleName
        public Guid? RoleId { get; set; } = null;
        public string? RoleName { get; set; } = null;

        // Either DepartmentId or DepartmentName
        public Guid? DepartmentId { get; set; } = null;
        public string? DepartmentName { get; set; } = null;
    }
}