using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.entities;
using AssetManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AssetDbContext _context;

        public AuditLogRepository(AssetDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .AsNoTracking()
                .Include(a => a.User)
                    .ThenInclude(u => u.Department)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            try
            {
                await _context.AuditLogs.AddAsync(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("AUDIT LOG ERROR: " + ex.Message);
            }
        }
    }
}