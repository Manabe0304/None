using AssetManagement.Domain.Common;
using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("broken_reports")]
    public class BrokenReport : BaseEntity
    {
        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("reported_by")]
        public Guid ReportedById { get; set; }
        [ForeignKey(nameof(ReportedById))]
        public User ReportedBy { get; set; } = null!;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("status")]
        public string Status { get; set; } = "OPEN";

        [Column("triage_result")]
        public string? TriageResult { get; set; }

        [Column("triage_by")]
        public Guid? TriageById { get; set; }
        [ForeignKey(nameof(TriageById))]
        public User? TriageBy { get; set; }

        [Column("triage_at")]
        public DateTime? TriageAt { get; set; }

        [Column("closure_reason")]
        public string? ClosureReason { get; set; }

        [Column("maintenance_id")]
        public Guid? MaintenanceId { get; set; }
        [ForeignKey(nameof(MaintenanceId))]
        public MaintenanceRecord? MaintenanceRecord { get; set; }

      
    }
}
