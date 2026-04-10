using AssetManagement.Domain.Common;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("assignments")]
    public class Assignment : BaseEntity
    {
        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("user_id")]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Column("request_id")]
        public Guid? RequestId { get; set; }
        public AssetRequest? Request { get; set; }

        [Column("assigned_by")]
        public Guid? AssignedById { get; set; }

        [ForeignKey(nameof(AssignedById))]
        public User? AssignedBy { get; set; }

        [Column("assigned_at")]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Column("returned_at")]
        public DateTime? ReturnedAt { get; set; }

        [Column("condition_notes")]
        public string? ConditionNotes { get; set; }

        [Column("is_temporary")]
        public bool IsTemporary { get; set; }

        [Column("status")]
        public string Status { get; set; } = AssignmentStatus.ASSIGNED.ToString();

        public ICollection<AssetReturnRequest> ReturnRequests { get; set; } = new List<AssetReturnRequest>();
        public ICollection<AssetHistory> Histories { get; set; } = new List<AssetHistory>();
    }
}
