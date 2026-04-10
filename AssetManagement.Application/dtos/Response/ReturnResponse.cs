using System;

namespace AssetManagement.Application.dtos.Response
{
    public class ReturnResponse
    {
        public Guid Id { get; set; }
        public Guid AssignmentId { get; set; }
        public Guid AssetId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ReceivedAt { get; set; }
        public DateTime? InspectedAt { get; set; }
        public string? HandbackCondition { get; set; }
    }
}
