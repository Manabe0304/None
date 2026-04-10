using AssetManagement.Application.dtos;
using AssetManagement.Application.dtos.Request.Department;
using AssetManagement.Application.interfaces.IRepository;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AssetDbContext _context;

        public DepartmentRepository(AssetDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Departments
                .Include(d => d.Manager)
                .Include(d => d.Users.Where(u => !u.IsDeleted))
                .Include(d => d.Assets.Where(a => !a.IsDeleted))
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(d => d.IsActive);
            }

            return await query
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Code = d.Code,
                    Description = d.Description,
                    IsActive = d.IsActive,
                    ManagerId = d.ManagerId,
                    ManagerName = d.Manager != null ? d.Manager.FullName : null,
                    IsDeleted = d.IsDeleted,
                    EmployeeCount = d.Users.Count(u => !u.IsDeleted),
                    TotalAssetCount = d.Assets.Count(a => !a.IsDeleted),
                    SharedAssetCount = d.Assets.Count(a => !a.IsDeleted && a.Status == "IN_USE_SHARED"),
                    UnassignedUserCountOnDelete = d.Users.Count(u => !u.IsDeleted),
                })
                .ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(Guid id)
        {
            return await _context.Departments
                .Include(d => d.Manager)
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
        }

        public async Task<DepartmentDto?> GetDetailsAsync(Guid id)
        {
            var department = await _context.Departments
                .AsNoTracking()
                .Include(d => d.Manager)
                .Include(d => d.Users.Where(u => !u.IsDeleted))
                    .ThenInclude(u => u.Assignments.Where(a => !a.IsDeleted && a.ReturnedAt == null && a.Status == "ASSIGNED"))
                .Include(d => d.Assets.Where(a => !a.IsDeleted && a.Status == "IN_USE_SHARED"))
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (department == null) return null;

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Code = department.Code,
                Description = department.Description,
                IsActive = department.IsActive,
                ManagerId = department.ManagerId,
                ManagerName = department.Manager?.FullName,
                IsDeleted = department.IsDeleted,
                EmployeeCount = department.Users.Count,
                TotalAssetCount = department.Users.Sum(u => u.Assignments.Count) + department.Assets.Count,
                SharedAssetCount = department.Assets.Count,
                UnassignedUserCountOnDelete = department.Users.Count,
                Employees = department.Users
                    .OrderBy(u => u.FullName ?? u.Email)
                    .Select(u => new DepartmentEmployeeAssetDto
                    {
                        UserId = u.Id,
                        EmployeeName = u.FullName ?? u.Email,
                        Email = u.Email,
                        AssetCount = u.Assignments.Count,
                    })
                    .ToList(),
                SharedAssets = department.Assets
                    .OrderBy(a => a.AssetTag)
                    .Select(a => new DepartmentSharedAssetDto
                    {
                        AssetId = a.Id,
                        AssetTag = a.AssetTag,
                        AssetName = a.Model,
                        Status = a.Status,
                    })
                    .ToList(),
            };
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var normalized = name.Trim().ToLower();
            return await _context.Departments.AnyAsync(d =>
                !d.IsDeleted && d.Name.ToLower() == normalized && (!excludeId.HasValue || d.Id != excludeId.Value));
        }

        public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null)
        {
            var normalized = code.Trim().ToLower();
            return await _context.Departments.AnyAsync(d =>
                !d.IsDeleted && d.Code.ToLower() == normalized && (!excludeId.HasValue || d.Id != excludeId.Value));
        }

        public async Task<bool> ManagerExistsAsync(Guid managerId)
        {
            return await _context.Users.AnyAsync(u => u.Id == managerId && !u.IsDeleted);
        }

        public async Task<Department> CreateAsync(CreateDepartmentRequest request)
        {
            var department = new Department
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Code = request.Code.Trim().ToUpper(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                IsActive = request.IsActive,
                ManagerId = request.ManagerId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
            };

            await _context.Departments.AddAsync(department);
            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<Department?> UpdateAsync(Guid id, UpdateDepartmentRequest request)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            if (department == null) return null;

            department.Name = request.Name.Trim();
            department.Code = request.Code.Trim().ToUpper();
            department.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            department.IsActive = request.IsActive;
            department.ManagerId = request.ManagerId;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return department;
        }

        public async Task<bool> SetStatusAsync(Guid id, bool isActive)
        {
            var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
            if (department == null) return false;

            department.IsActive = isActive;
            department.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var department = await _context.Departments
                .Include(d => d.Users.Where(u => !u.IsDeleted))
                .Include(d => d.Assets.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (department == null) return false;

            var now = DateTime.UtcNow;

            foreach (var user in department.Users)
            {
                user.DepartmentId = null;
                user.UpdatedAt = now;
            }

            foreach (var asset in department.Assets.Where(a => a.Status == "IN_USE_SHARED"))
            {
                asset.DepartmentId = null;
                asset.Status = "AVAILABLE";
                asset.UpdatedAt = now;
            }

            department.IsDeleted = true;
            department.UpdatedAt = now;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}