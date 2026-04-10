using AssetManagement.Application.dtos;
using AssetManagement.Application.dtos.Request.User;
using AssetManagement.Application.dtos.Response.User;
using AssetManagement.Application.interfaces;
using AssetManagement.Application.interfaces.IRepository;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<UserResponse>> GetUsersAsync(int page, string? search)
        {
            return await _repository.GetPagedAsync(page, search);
        }

        public async Task<UserDetailResponse?> GetUserByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null) return null;

            return new UserDetailResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name,
                Status = user.Status
            };
        }

        public async Task CreateUserAsync(CreateUserRequest request)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = request.Password,
                FullName = request.FullName,
                RoleId = request.RoleId,
                DepartmentId = request.DepartmentId,
                Status = "ACTIVE",
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(user);
        }

        public async Task UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("User not found");

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.PasswordHash = request.Password;

            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
                user.DepartmentId = request.DepartmentId.Value;

            if (request.RoleId.HasValue && request.RoleId.Value != Guid.Empty)
                user.RoleId = request.RoleId.Value;

            user.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);

            if (user == null)
                throw new Exception("User not found");

            await _repository.DeleteAsync(user);
        }
    }
}