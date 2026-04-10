using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.dtos.Request.AssetRequests
{
    public class RejectAssetRequestRequest
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [MaxLength(100, ErrorMessage = "Rejection reason must not exceed 100 characters.")]
        public string Reason { get; set; } = string.Empty;
    }
}