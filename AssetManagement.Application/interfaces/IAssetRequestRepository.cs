using AssetManagement.Domain.Entities;
using AssetManagement.Domain.entities;

namespace AssetManagement.Application.interfaces
{
    public interface IAssetRequestRepository
    {
        Task<User?> GetEmployeeForRequestAsync(Guid userId);
        Task AddAssetRequestAsync(AssetRequest request);
        Task AddNotificationAsync(Notification notification);
        Task<List<AssetRequest>> GetMyRequestsAsync(Guid employeeId);

        // Manager
        Task<User?> GetManagerWithDepartmentAsync(Guid managerId);
        Task<List<AssetRequest>> GetPendingRequestsByDepartmentAsync(Guid departmentId);
        Task<AssetRequest?> GetRequestForManagerDecisionAsync(Guid requestId, Guid managerDepartmentId);

        Task SaveChangesAsync();
    }
}