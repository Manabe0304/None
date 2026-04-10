using AssetManagement.Application.dtos;
using AssetManagement.Application.dtos.Request.Department;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.interfaces.IRepository
{
    public interface IDepartmentRepository
    {
        Task<List<DepartmentDto>> GetAllAsync(bool includeInactive = false);
        Task<Department?> GetByIdAsync(Guid id);
        Task<DepartmentDto?> GetDetailsAsync(Guid id);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
        Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null);
        Task<bool> ManagerExistsAsync(Guid managerId);
        Task<Department> CreateAsync(CreateDepartmentRequest request);
        Task<Department?> UpdateAsync(Guid id, UpdateDepartmentRequest request);
        Task<bool> SetStatusAsync(Guid id, bool isActive);
        Task<bool> DeleteAsync(Guid id);
    }
}