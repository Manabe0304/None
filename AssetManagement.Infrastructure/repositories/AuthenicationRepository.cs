using AssetManagement.Application.interfaces.IRepository;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class AuthenicationRepository(AssetDbContext _context) : IAuthenicationRepository
    {
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(x => x.Role)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email);
        }

        public async Task<bool> RoleExistsAsync(Guid roleId)
        {
            return await _context.Roles.AnyAsync(x => x.Id == roleId);
        }

        public async Task<bool> DepartmentExistsAsync(Guid departmentId)
        {
            return await _context.Departments.AnyAsync(x => x.Id == departmentId);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}