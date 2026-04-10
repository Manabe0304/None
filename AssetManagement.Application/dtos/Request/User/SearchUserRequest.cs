namespace AssetManagement.Application.dtos.Request.User
{
    public class SearchUserRequest
    {
        public int Page { get; set; } = 1;
        public string? Search { get; set; }
    }
}