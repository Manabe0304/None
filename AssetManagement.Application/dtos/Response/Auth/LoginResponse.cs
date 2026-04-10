namespace AssetManagement.Application.dtos.Response.Auth
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public string? DepartmentName { get; set; }
        public string? Token { get; set; }
    }
}