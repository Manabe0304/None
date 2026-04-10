using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.entities;

namespace AssetManagement.Application.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _auditLogRepository.GetAllAsync();
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            await _auditLogRepository.AddAsync(auditLog);
        }
    }
}