using AssetManagement.Application.dtos.Response;
using AssetManagement.Application.dtos.Response.ReturnRequests;
using AssetManagement.Application.interfaces.IRepository;

namespace AssetManagement.Application.services
{
    public class ReturnRequestService
    {
        private readonly IReturnRequestRepository _returnRequestRepository;

        public ReturnRequestService(IReturnRequestRepository returnRequestRepository)
        {
            _returnRequestRepository = returnRequestRepository;
        }

        public async Task<ApiResponse<List<ReturnedAssetHistoryItemResponse>>> GetMyReturnedHistoryAsync(Guid employeeId)
        {
            var items = await _returnRequestRepository.GetMyReturnedHistoryAsync(employeeId);

            var data = items.Select(r => new ReturnedAssetHistoryItemResponse
            {
                Id = r.Id,
                AssetTag = r.Asset.AssetTag,
                AssetType = r.Asset.Category ?? string.Empty,
                ReturnInitiatedDate = r.CreatedAt,
                ReturnReason = r.ReturnReason,
                Status = r.Status,
                ProcessedDate = r.HandledAt
            }).ToList();

            return new ApiResponse<List<ReturnedAssetHistoryItemResponse>>
            {
                StatusCode = 200,
                Message = "Returned asset history retrieved successfully",
                Data = data
            };
        }

        public async Task<ApiResponse<ReturnedAssetHistoryDetailResponse>> GetMyReturnedHistoryDetailAsync(Guid employeeId, Guid returnRequestId)
        {
            var item = await _returnRequestRepository.GetMyReturnedHistoryDetailAsync(employeeId, returnRequestId);

            if (item == null)
            {
                return new ApiResponse<ReturnedAssetHistoryDetailResponse>
                {
                    StatusCode = 404,
                    Message = "Return record not found",
                    Data = null
                };
            }

            return new ApiResponse<ReturnedAssetHistoryDetailResponse>
            {
                StatusCode = 200,
                Message = "Return record detail retrieved successfully",
                Data = new ReturnedAssetHistoryDetailResponse
                {
                    Id = item.Id,
                    AssetTag = item.Asset.AssetTag,
                    AssetType = item.Asset.Category ?? string.Empty,
                    ReturnInitiatedDate = item.CreatedAt,
                    ReturnReason = item.ReturnReason,
                    Status = item.Status,
                    ProcessedDate = item.HandledAt,
                    HandlingNotes = item.Notes,
                    InitialReceptionResult = item.ConditionAtHandback,
                    HandledByName = item.HandledBy?.FullName,
                    AssignmentId = item.AssignmentId,
                    AssignedAt = item.Assignment.AssignedAt,
                    ReturnedAt = item.Assignment.ReturnedAt
                }
            };
        }
    }
}