using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("asset_histories")]
    public class AssetHistory
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("assignment_id")]
        public Guid? AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }

        [Column("user_id")]
        public Guid? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Column("changed_by_id")]
        public Guid? ChangedById { get; set; }

        [ForeignKey(nameof(ChangedById))]
        public User? ChangedBy { get; set; }

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("previous_status")]
        public string? PreviousStatus { get; set; }

        [Column("new_status")]
        public string? NewStatus { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
