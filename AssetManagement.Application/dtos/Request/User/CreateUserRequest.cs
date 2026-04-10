namespace AssetManagement.Application.dtos.Request.User
{
    public class CreateUserRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FullName { get; set; }

        // Either provide RoleId or RoleName
        public Guid? RoleId { get; set; }
        public string? RoleName { get; set; }

        // Either provide DepartmentId or DepartmentName
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}