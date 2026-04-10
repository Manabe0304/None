using AssetManagement.Application.dtos.Request.AssetRequests;
using AssetManagement.Application.dtos.Response;
using AssetManagement.Application.dtos.Response.AssetRequests;
using AssetManagement.Application.interfaces;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.entities;

namespace AssetManagement.Application.services
{
    public class AssetRequestService
    {
        private readonly IAssetRequestRepository _assetRequestRepository;

        private static readonly HashSet<string> AllowedAssetTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "LAPTOP",
            "MONITOR",
            "KEYBOARD",
            "MOUSE",
            "HEADSET",
            "DOCKING_STATION",
            "PRINTER"
        };

        private static readonly HashSet<string> AllowedUrgencyLevels = new(StringComparer.OrdinalIgnoreCase)
        {
            "LOW",
            "MEDIUM",
            "HIGH",
            "CRITICAL"
        };

        public AssetRequestService(IAssetRequestRepository assetRequestRepository)
        {
            _assetRequestRepository = assetRequestRepository;
        }

        public async Task<ApiResponse<AssetRequestResponse>> SubmitRequestAsync(
            Guid employeeId,
            SubmitAssetRequestRequest request)
        {
            var employee = await _assetRequestRepository.GetEmployeeForRequestAsync(employeeId);

            if (employee == null)
            {
                return new ApiResponse<AssetRequestResponse>
                {
                    StatusCode = 404,
                    Message = "Employee not found",
                    Data = null
                };
            }

            if (!string.Equals(employee.Role?.Name, "EMPLOYEE", StringComparison.OrdinalIgnoreCase))
            {
                return new ApiResponse<AssetRequestResponse>
                {
                    StatusCode = 403,
                    Message = "Only employees can submit asset requests",
                    Data = null
                };
            }

            if (employee.DepartmentId == null || employee.Department == null)
            {
                return new ApiResponse<AssetRequestResponse>
                {
                    StatusCode = 400,
                    Message = "Employee is not assigned to any department",
                    Data = null
                };
            }

            if (!employee.Department.IsActive)
            {
                return new ApiResponse<AssetRequestResponse>
                {
                    StatusCode = 400,
                    Message = "Employee department is inactive",
                    Data = null
                };
            }

            var managerId = employee.Department.ManagerId;

            if (managerId == null || managerId == Guid.Empty)
            {
                return new ApiResponse<AssetRequestResponse>
                {
                    StatusCode = 400,
                    Message = "Department manager is not assigned",
                    Data = null
                };
            }

            var normalizedAssetType = request.AssetType.Trim().ToUpperInvariant();
            var normalizedUrgency = request.UrgencyLevel.Trim().ToUpperInvariant();

            if (!AllowedAssetTypes.Contains(normalizedAssetType))
            {
                return new ApiResponse<AssetRequestResponse>
                {
                    StatusCode = 400,
                    Message = "Invalid asset type",
                    Data = null
                };
            }

            if (!AllowedUrgencyLevels.Contains(normalizedUrgency))
            {
                return new ApiResponse<AssetRequestResponse>
                {
                    StatusCode = 400,
                    Message = "Invalid urgency level",
                    Data = null
                };
            }

            var now = DateTime.UtcNow;

            var assetRequest = new AssetRequest
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                AssetType = normalizedAssetType,
                PreferredModel = string.IsNullOrWhiteSpace(request.PreferredModel)
                    ? null
                    : request.PreferredModel.Trim(),
                Reason = request.Reason.Trim(),
                UrgencyLevel = normalizedUrgency,
                RequestType = "STANDARD",
                Status = AssetRequestStatuses.Pending,
                ManagerId = managerId.Value,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _assetRequestRepository.AddAssetRequestAsync(assetRequest);

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = managerId.Value,
                Title = "New asset request pending review",
                Message = $"{employee.FullName ?? employee.Email} submitted a {normalizedAssetType} request with {normalizedUrgency} urgency.",
                IsRead = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _assetRequestRepository.AddNotificationAsync(notification);
            await _assetRequestRepository.SaveChangesAsync();

            return new ApiResponse<AssetRequestResponse>
            {
                StatusCode = 201,
                Message = "Asset request submitted successfully",
                Data = new AssetRequestResponse
                {
                    Id = assetRequest.Id,
                    EmployeeId = assetRequest.EmployeeId,
                    ManagerId = assetRequest.ManagerId,
                    AssetType = assetRequest.AssetType ?? string.Empty,
                    PreferredModel = assetRequest.PreferredModel,
                    Reason = assetRequest.Reason ?? string.Empty,
                    UrgencyLevel = assetRequest.UrgencyLevel,
                    Status = assetRequest.Status,
                    CreatedAt = assetRequest.CreatedAt
                }
            };
        }

        public async Task<ApiResponse<List<MyAssetRequestItemResponse>>> GetMyRequestsAsync(Guid employeeId)
        {
            var requests = await _assetRequestRepository.GetMyRequestsAsync(employeeId);

            var data = requests.Select(r =>
            {
                var latestAssignment = r.Assignments
                    .OrderByDescending(a => a.AssignedAt)
                    .FirstOrDefault();

                return new MyAssetRequestItemResponse
                {
                    Id = r.Id,
                    RequestDate = r.CreatedAt,
                    RequestedDeviceType = r.AssetType ?? string.Empty,
                    PreferredModel = r.PreferredModel,
                    CurrentStatus = r.Status,
                    RejectionReason = string.Equals(r.Status, "REJECTED", StringComparison.OrdinalIgnoreCase)
                        ? r.RejectionReason
                        : null,
                    Assignment = latestAssignment == null ? null : new AssignmentInfoResponse
                    {
                        AssignmentId = latestAssignment.Id,
                        AssetId = latestAssignment.AssetId,
                        AssetTag = latestAssignment.Asset?.AssetTag,
                        AssetCategory = latestAssignment.Asset?.Category,
                        AssignedAt = latestAssignment.AssignedAt,
                        ReturnedAt = latestAssignment.ReturnedAt,
                        Status = latestAssignment.Status
                    }
                };
            }).ToList();

            return new ApiResponse<List<MyAssetRequestItemResponse>>
            {
                StatusCode = 200,
                Message = "Asset request history retrieved successfully",
                Data = data
            };
        }
    }
}