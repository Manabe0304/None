namespace AssetManagement.Application.dtos.Response.User
{
    public class UserDetailResponse
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;
        public string? FullName { get; set; }

        public Guid? RoleId { get; set; }
        public string? RoleName { get; set; }

        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }

        public string Status { get; set; } = null!;
    }
}