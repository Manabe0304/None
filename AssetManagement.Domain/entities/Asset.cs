using AssetManagement.Domain.Common;
using AssetManagement.Domain.entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.Entities
{
    [Table("assets")]
    public class Asset : BaseEntity
    {
        [Column("asset_tag")]
        public string AssetTag { get; set; } = null!;

        [Column("category")]
        public string? Category { get; set; }

        [Column("model")]
        public string? Model { get; set; }

        [Column("serial_number")]
        public string? SerialNumber { get; set; }

        [Column("purchase_date")]
        public DateTime? PurchaseDate { get; set; }

        [Column("purchase_value")]
        public decimal? PurchaseValue { get; set; }

        [Column("status")]
        public string Status { get; set; } = "AVAILABLE";

        [Column("department_id")]
        public Guid? DepartmentId { get; set; }

        public Department? Department { get; set; }

        [Column("current_user_id")]
        public Guid? CurrentUserId { get; set; }
        public User? CurrentUser { get; set; }

        [Column("is_beyond_repair")]
        public bool IsBeyondRepair { get; set; }

        [Column("beyond_repair_reason")]
        public string? BeyondRepairReason { get; set; }

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
        public ICollection<AssetWarranty> Warranties { get; set; } = new List<AssetWarranty>();
        public ICollection<AssetReturnRequest> ReturnRequests { get; set; } = new List<AssetReturnRequest>();
        public ICollection<BrokenReport> BrokenReports { get; set; } = new List<BrokenReport>();
        public ICollection<AssetHistory> Histories { get; set; } = new List<AssetHistory>();
        public ICollection<AssetInspection> Inspections { get; set; } = new List<AssetInspection>();
        public ICollection<AssetReservation> Reservations { get; set; } = new List<AssetReservation>();
        public AssetLiquidation? Liquidation { get; set; }
    }
}
