using AssetManagement.Domain.entities;

namespace AssetManagement.Application.interfaces
{
    public interface IBrokenReportRepository
    {
        Task<List<BrokenReport>> GetAllAsync(string? search);
        Task<BrokenReport?> GetByIdAsync(Guid id);
        Task<bool> AcceptReportAsync(Guid reportId, Guid adminId);
        Task<bool> RejectReportAsync(Guid reportId, Guid adminId, string? reason);
    }
}