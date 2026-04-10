using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.interfaces.IRepository
{
    public interface IAuthenicationRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> RoleExistsAsync(Guid roleId);
        Task<bool> DepartmentExistsAsync(Guid departmentId);
        Task<User> CreateUserAsync(User user);
        Task SaveChangesAsync();
    }
}