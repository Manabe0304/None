namespace AssetManagement.Application.dtos.Response.User
{
    public class UserResponse
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;
        public string? FullName { get; set; }

        public string? RoleName { get; set; }
        public string? DepartmentName { get; set; }

        public string Status { get; set; } = null!;
    }
}