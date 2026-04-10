using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;

namespace AssetManagement.Application.services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentRepository _assignmentRepository;

        public AssignmentService(IAssignmentRepository assignmentRepository)
        {
            _assignmentRepository = assignmentRepository;
        }

        public Task<List<MyAssignmentDto>> GetAssignmentsByUserIdAsync(Guid userId)
        {
            // Gọi xuống repository để lấy dữ liệu
            return _assignmentRepository.GetAssignmentsByUserIdAsync(userId);
        }
        public Task<List<AvailableAssetDto>> GetAvailableAssetsAsync()
            => _assignmentRepository.GetAvailableAssetsAsync();

        public Task<List<ApprovedRequestDto>> GetApprovedRequestsAsync()
            => _assignmentRepository.GetApprovedRequestsAsync();

        public async Task<AssignmentResultDto> AssignAsync(AssignmentCreateDto dto, Guid adminId)
        {
            if (dto.RequestId == null || dto.RequestId == Guid.Empty)
            {
                throw new ArgumentException("RequestId là bắt buộc.");
            }

            if (dto.AssetId == Guid.Empty)
            {
                throw new ArgumentException("AssetId là bắt buộc.");
            }

            if (adminId == Guid.Empty)
            {
                throw new ArgumentException("AssignedById không hợp lệ.");
            }

            return await _assignmentRepository.ExecuteInTransactionAsync(async () =>
            {
                var now = DateTime.UtcNow;

                var assetRequest = await _assignmentRepository.GetRequestForAssignmentAsync(dto.RequestId.Value);
                if (assetRequest == null)
                {
                    throw new KeyNotFoundException("Request không tồn tại.");
                }

                var validRequestStatuses = new[]
                {
                    AssetRequestStatuses.Approved,
                    AssetRequestStatuses.PendingAdminAssignment
                };

                if (!validRequestStatuses.Contains(assetRequest.Status, StringComparer.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Request chưa ở trạng thái được phép assignment.");
                }

                var asset = await _assignmentRepository.GetAssetByIdAsync(dto.AssetId);
                if (asset == null || asset.IsDeleted)
                {
                    throw new KeyNotFoundException("Asset không tồn tại.");
                }

                var availableStatus = AssetStatus.AVAILABLE.ToString();
                var inUseStatus = AssetStatus.IN_USE.ToString();
                var assignedStatus = AssignmentStatus.ASSIGNED.ToString();

                if (!string.Equals(asset.Status, availableStatus, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Asset không ở trạng thái AVAILABLE.");
                }

                if (!string.IsNullOrWhiteSpace(assetRequest.AssetType)
                    && !string.Equals(asset.Category, assetRequest.AssetType, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Asset không đúng loại request.");
                }

                var hasOpenAssignment = await _assignmentRepository.HasOpenAssignmentAsync(asset.Id, assignedStatus);
                if (hasOpenAssignment)
                {
                    throw new InvalidOperationException("Asset đang có assignment hoạt động.");
                }

                var assignment = new Assignment
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    UserId = assetRequest.EmployeeId,
                    RequestId = assetRequest.Id,
                    AssignedById = adminId,
                    AssignedAt = now,
                    ConditionNotes = dto.Note,
                    Status = assignedStatus,
                    CreatedAt = now
                };

                var previousAssetStatus = asset.Status;
                asset.CurrentUserId = assetRequest.EmployeeId;
                asset.Status = inUseStatus;
                asset.UpdatedAt = now;

                assetRequest.Status = AssetRequestStatuses.Assigned;
                assetRequest.UpdatedAt = now;

                await _assignmentRepository.AddAssignmentAsync(assignment);

                await _assignmentRepository.AddAssetHistoryAsync(new AssetHistory
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    AssignmentId = assignment.Id,
                    UserId = assetRequest.EmployeeId,
                    ChangedById = adminId,
                    Action = assignedStatus,
                    PreviousStatus = previousAssetStatus,
                    NewStatus = asset.Status,
                    Note = dto.Note ?? $"Asset {asset.AssetTag} assigned to {assetRequest.Employee?.FullName ?? assetRequest.Employee?.Email}",
                    ChangedAt = now,
                    CreatedAt = now
                });

                await _assignmentRepository.AddAuditLogAsync(new AuditLog
                {
                    Id = Guid.NewGuid(),
                    UserId = adminId,
                    Action = assignedStatus,
                    EntityType = "ASSIGNMENT",
                    EntityId = assignment.Id,
                    CreatedAt = now
                });

                await _assignmentRepository.AddNotificationAsync(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = assetRequest.EmployeeId,
                    Title = "Asset assigned",
                    Message = $"Asset {asset.AssetTag} has been assigned to you.",
                    IsRead = false,
                    CreatedAt = now
                });

                return new AssignmentResultDto
                {
                    AssignmentId = assignment.Id,
                    Message = "Gán tài sản thành công"
                };
            });
        }

        public Task<List<CurrentAssignmentDto>> GetCurrentAssignmentsAsync(string? employee = null, string? department = null, string? assetType = null)
            => _assignmentRepository.GetCurrentAssignmentsAsync(employee, department, assetType);

        public Task<List<AssetHistoryItemDto>> GetAssetHistoryAsync(
            string? assetKeyword = null,
            string? employee = null,
            string? department = null,
            string? assetType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
            => _assignmentRepository.GetAssetHistoryAsync(assetKeyword, employee, department, assetType, fromDate, toDate);
    }
}
