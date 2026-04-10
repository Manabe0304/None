using AssetManagement.Domain.Common;
using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("return_requests")]
    public class AssetReturnRequest : BaseEntity
    {
        [Column("assignment_id")]
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;

        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("user_id")]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Column("initiated_by")]
        public Guid InitiatedById { get; set; }

        [ForeignKey(nameof(InitiatedById))]
        public User InitiatedBy { get; set; } = null!;

        [Column("return_reason")]
        public string? ReturnReason { get; set; }

        [Column("status")]
        public string Status { get; set; } = "PENDING";

        [Column("expected_return_date")]
        public DateTime? ExpectedReturnDate { get; set; }

        [Column("condition_at_handback")]
        public string? ConditionAtHandback { get; set; }

        [Column("handled_by")]
        public Guid? HandledById { get; set; }

        [ForeignKey(nameof(HandledById))]
        public User? HandledBy { get; set; }

        [Column("handled_at")]
        public DateTime? HandledAt { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }
    }
}
