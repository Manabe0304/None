using AssetManagement.Domain.entities;

namespace AssetManagement.Application.Interfaces
{
    public interface IAuditLogService
    {
        Task<List<AuditLog>> GetAllAsync();
        Task AddAsync(AuditLog auditLog);
    }
}