using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("audit_logs")]
    public class AuditLog
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid? UserId { get; set; }
        public User? User { get; set; }

        [Column("actor_email")]
        public string? ActorEmail { get; set; }

        [Column("actor_department")]
        public string? ActorDepartment { get; set; }

        [Column("target_email")]
        public string? TargetEmail { get; set; }

        [Column("action")]
        public string? Action { get; set; }

        [Column("entity_type")]
        public string? EntityType { get; set; }

        [Column("entity_id")]
        public Guid? EntityId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}