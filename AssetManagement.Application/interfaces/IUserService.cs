using AssetManagement.Application.dtos;
using AssetManagement.Application.dtos.Request.User;
using AssetManagement.Application.dtos.Response.User;

namespace AssetManagement.Application.interfaces
{
    public interface IUserService
    {
        Task<PagedResult<UserResponse>> GetUsersAsync(int page, string? search);
        Task<UserDetailResponse?> GetUserByIdAsync(Guid id);
        Task CreateUserAsync(CreateUserRequest request);
        Task UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task DeleteUserAsync(Guid id);
    }
}