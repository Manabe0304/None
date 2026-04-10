using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("asset_reservations")]
    public class AssetReservation
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("request_id")]
        public Guid RequestId { get; set; }
        public AssetRequest Request { get; set; } = null!;

        [Column("reserved_by")]
        public Guid ReservedById { get; set; }

        [ForeignKey(nameof(ReservedById))]
        public User ReservedBy { get; set; } = null!;

        [Column("reserved_at")]
        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

        [Column("expires_at")]
        public DateTime? ExpiresAt { get; set; }

        [Column("expected_assign_date")]
        public DateTime? ExpectedAssignDate { get; set; }

        [Column("status")]
        public string Status { get; set; } = "ACTIVE";

        [Column("note")]
        public string? Note { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
