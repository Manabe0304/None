using AssetManagement.Application.dtos.Request.AssetRequests;
using AssetManagement.Application.dtos.Response;
using AssetManagement.Application.dtos.Response.AssetRequests;
using AssetManagement.Application.interfaces;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.services
{
    public class ManagerApprovalService
    {
        private readonly IAssetRequestRepository _assetRequestRepository;

        public ManagerApprovalService(IAssetRequestRepository assetRequestRepository)
        {
            _assetRequestRepository = assetRequestRepository;
        }

        public async Task<ApiResponse<List<ManagerApprovalItemResponse>>> GetApprovalRequestsAsync(Guid managerId)
        {
            var manager = await _assetRequestRepository.GetManagerWithDepartmentAsync(managerId);

            if (manager == null)
            {
                return new ApiResponse<List<ManagerApprovalItemResponse>>
                {
                    StatusCode = 404,
                    Message = "Manager not found",
                    Data = null
                };
            }

            if (!string.Equals(manager.Role?.Name, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<List<ManagerApprovalItemResponse>>
                {
                    StatusCode = 403,
                    Message = "Only managers can review requests",
                    Data = null
                };
            }

            if (manager.DepartmentId == null)
            {
                return new ApiResponse<List<ManagerApprovalItemResponse>>
                {
                    StatusCode = 400,
                    Message = "Manager is not assigned to any department",
                    Data = null
                };
            }

            var requests = await _assetRequestRepository.GetPendingRequestsByDepartmentAsync(manager.DepartmentId.Value);

            var data = requests.Select(r => new ManagerApprovalItemResponse
            {
                Id = r.Id,
                EmployeeName = r.Employee?.FullName ?? r.Employee?.Email ?? string.Empty,
                Department = r.Employee?.Department?.Name ?? manager.Department?.Name ?? "N/A",
                AssetName = r.AssetType ?? string.Empty,
                Reason = r.Reason ?? string.Empty,
                Urgency = r.UrgencyLevel,
                Status = r.Status,
                RequestDate = r.CreatedAt
            }).ToList();

            return new ApiResponse<List<ManagerApprovalItemResponse>>
            {
                StatusCode = 200,
                Message = "Approval requests retrieved successfully",
                Data = data
            };
        }

        public async Task<ApiResponse<object>> ApproveRequestAsync(Guid managerId, Guid requestId)
        {
            var manager = await _assetRequestRepository.GetManagerWithDepartmentAsync(managerId);

            if (manager == null)
            {
                return new ApiResponse<object> { StatusCode = 404, Message = "Manager not found" };
            }

            if (!string.Equals(manager.Role?.Name, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object> { StatusCode = 403, Message = "Only managers can approve requests" };
            }

            if (manager.DepartmentId == null)
            {
                return new ApiResponse<object> { StatusCode = 400, Message = "Manager is not assigned to any department" };
            }

            var request = await _assetRequestRepository.GetRequestForManagerDecisionAsync(requestId, manager.DepartmentId.Value);

            if (request == null)
            {
                return new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = "Request not found or not under manager's department"
                };
            }

            request.Status = AssetRequestStatuses.Approved;
            request.ManagerId = manager.Id;
            request.DecisionAt = DateTime.UtcNow;
            request.RejectionReason = null;
            request.UpdatedAt = DateTime.UtcNow;

            await _assetRequestRepository.AddNotificationAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.EmployeeId,
                Title = "Asset request approved",
                Message = $"Your request for {request.AssetType} has been approved by manager and moved to admin assignment queue.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _assetRequestRepository.SaveChangesAsync();

            return new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Request moved to admin assignment queue"
            };
        }

        public async Task<ApiResponse<object>> RejectRequestAsync(Guid managerId, Guid requestId, RejectAssetRequestRequest rejectRequest)
        {
            var manager = await _assetRequestRepository.GetManagerWithDepartmentAsync(managerId);

            if (manager == null)
            {
                return new ApiResponse<object> { StatusCode = 404, Message = "Manager not found" };
            }

            if (!string.Equals(manager.Role?.Name, "MANAGER", StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<object> { StatusCode = 403, Message = "Only managers can reject requests" };
            }

            if (manager.DepartmentId == null)
            {
                return new ApiResponse<object> { StatusCode = 400, Message = "Manager is not assigned to any department" };
            }

            var request = await _assetRequestRepository.GetRequestForManagerDecisionAsync(requestId, manager.DepartmentId.Value);

            if (request == null)
            {
                return new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = "Request not found or not under manager's department"
                };
            }

            if (string.IsNullOrWhiteSpace(rejectRequest.Reason))
            {
                return new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = "Rejection reason is required"
                };
            }

            request.Status = AssetRequestStatuses.Rejected;
            request.ManagerId = manager.Id;
            request.DecisionAt = DateTime.UtcNow;
            request.RejectionReason = rejectRequest.Reason.Trim();
            request.UpdatedAt = DateTime.UtcNow;

            await _assetRequestRepository.AddNotificationAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.EmployeeId,
                Title = "Asset request rejected",
                Message = $"Your request for {request.AssetType} was rejected. Reason: {request.RejectionReason}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _assetRequestRepository.SaveChangesAsync();

            return new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Request rejected successfully"
            };
        }
    }
}