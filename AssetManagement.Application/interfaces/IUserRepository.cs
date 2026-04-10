using AssetManagement.Application.dtos;
using AssetManagement.Application.dtos.Response.User;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.interfaces.IRepository
{
    public interface IUserRepository
    {
        Task<PagedResult<UserResponse>> GetPagedAsync(int page, string? search);

        Task<User?> GetByIdAsync(Guid id);

        Task AddAsync(User user);

        Task UpdateAsync(User user);

        Task DeleteAsync(User user);
    }
}