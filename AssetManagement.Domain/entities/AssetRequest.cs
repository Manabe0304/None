using AssetManagement.Domain.Common;
using AssetManagement.Domain.Constants;
using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("asset_requests")]
    public class AssetRequest : BaseEntity
    {
        [Column("employee_id")]
        public Guid EmployeeId { get; set; }
        public User Employee { get; set; } = null!;

        [Column("asset_type")]
        public string? AssetType { get; set; }

        [Column("preferred_model")]
        public string? PreferredModel { get; set; }

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("urgency_level")]
        public string UrgencyLevel { get; set; } = "MEDIUM";

        [Column("request_type")]
        public string RequestType { get; set; } = "STANDARD";

        [Column("status")]
        public string Status { get; set; } = AssetRequestStatuses.Pending;

        [Column("rejection_reason")]
        public string? RejectionReason { get; set; }

        [Column("manager_id")]
        public Guid? ManagerId { get; set; }
        public User? Manager { get; set; }

        [Column("decision_at")]
        public DateTime? DecisionAt { get; set; }

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<AssetReservation> Reservations { get; set; } = new List<AssetReservation>();
    }
}
