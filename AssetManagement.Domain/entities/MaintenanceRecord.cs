using AssetManagement.Domain.Common;
using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("maintenance_records")]
    public class MaintenanceRecord : BaseEntity
    {
        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("broken_report_id")]
        public Guid? BrokenReportId { get; set; }
        public BrokenReport? BrokenReport { get; set; }

        [Column("reported_by")]
        public Guid? ReportedBy { get; set; }
        public User? Reporter { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("estimated_cost")]
        public decimal? EstimatedCost { get; set; }

        [Column("repair_cost")]
        public decimal? RepairCost { get; set; }

        [Column("outcome")]
        public string? Outcome { get; set; }

        [Column("status")]
        public string? Status { get; set; }

        [Column("vendor_id")]
        public Guid? VendorId { get; set; }
        public Vendor? Vendor { get; set; }

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }
    }
}
