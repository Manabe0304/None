using AssetManagement.Domain.Common;
using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("return_requests")]
    public class ReturnRequest : BaseEntity
    {
        [Column("assignment_id")]
        public Guid? AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }

        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("requested_by")]
        public Guid RequestedById { get; set; }
        public User RequestedBy { get; set; } = null!;

        [Column("status")]
        public string Status { get; set; } = "REQUESTED";

        [Column("received_at")]
        public DateTime? ReceivedAt { get; set; }

        [Column("inspected_at")]
        public DateTime? InspectedAt { get; set; }

        [Column("handback_condition")]
        public string? HandbackCondition { get; set; }
    }
}
