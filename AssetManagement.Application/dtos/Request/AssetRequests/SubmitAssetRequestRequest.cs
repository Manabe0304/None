using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.dtos.Request.AssetRequests
{
    public class SubmitAssetRequestRequest
    {
        [Required(ErrorMessage = "AssetType")]
        public string AssetType { get; set; } = string.Empty;

        public string? PreferredModel { get; set; }

        [Required(ErrorMessage = "Reason")]
        [MinLength(10, ErrorMessage = "The reason why it must have at least 10 characters.")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Urgency level")]
        public string UrgencyLevel { get; set; } = string.Empty;
    }
}