using AssetManagement.Application.dtos;
using AssetManagement.Application.dtos.Response.User;
using AssetManagement.Application.interfaces.IRepository;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AssetDbContext _context;

        public UserRepository(AssetDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<UserResponse>> GetPagedAsync(int page, string? search)
        {
            int pageSize = 10;

            var query = _context.Users
                .Include(x => x.Role)
                .Include(x => x.Department)
                .Where(x => !x.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                // Improve substring matching:
                // - split tokens by space and require all tokens (AND) to be present somewhere
                // - also match FullName with spaces removed so searching 'an' or 'hoaian' matches 'Hoai An'
                var s = search.Trim();
                var tokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var token in tokens)
                {
                    var pattern = $"%{token}%";
                    query = query.Where(x =>
                        EF.Functions.ILike(x.Email, pattern) ||
                        (x.FullName != null && EF.Functions.ILike(x.FullName, pattern)) ||
                        // match fullname with spaces removed
                        (x.FullName != null && EF.Functions.ILike(x.FullName.Replace(" ", ""), pattern)) ||
                        (x.Role != null && EF.Functions.ILike(x.Role.Name, pattern)) ||
                        (x.Department != null && EF.Functions.ILike(x.Department.Name, pattern))
                    );
                }
            }

            var total = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserResponse
                {
                    Id = x.Id,
                    Email = x.Email,
                    FullName = x.FullName,
                    RoleName = x.Role!.Name,
                    DepartmentName = x.Department!.Name,
                    Status = x.Status
                })
                .ToListAsync();

            return new PagedResult<UserResponse>
            {
                Items = users,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .Include(x => x.Role)
                .Include(x => x.Department)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == user.Id);

            if (existing == null)
                throw new Exception("User not found");

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.UpdatedAt = DateTime.UtcNow;

            if (user.RoleId != null)
                existing.RoleId = user.RoleId;

            if (user.DepartmentId != null)
                existing.DepartmentId = user.DepartmentId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            user.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}