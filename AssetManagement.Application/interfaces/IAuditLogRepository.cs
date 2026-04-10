using AssetManagement.Domain.entities;

namespace AssetManagement.Application.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<List<AuditLog>> GetAllAsync();
        Task AddAsync(AuditLog auditLog);
    }
}