using AssetManagement.Domain.entities;

namespace AssetManagement.Application.interfaces.IRepository
{
    public interface IReturnRequestRepository
    {
        Task<List<AssetReturnRequest>> GetMyReturnedHistoryAsync(Guid employeeId);
        Task<AssetReturnRequest?> GetMyReturnedHistoryDetailAsync(Guid employeeId, Guid returnRequestId);
    }
}