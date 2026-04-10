using AssetManagement.Domain.Common;
using AssetManagement.Domain.entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.Entities
{
    [Table("users")]
    public class User : BaseEntity
    {
        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = null!;

        [Column("full_name")]
        public string? FullName { get; set; }

        [Column("role_id")]
        public Guid? RoleId { get; set; }
        public Role? Role { get; set; }

        [Column("department_id")]
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }

        [Column("status")]
        public string Status { get; set; } = "ACTIVE";

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<AssetRequest> AssetRequests { get; set; } = new List<AssetRequest>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<AssetReturnRequest> ReturnRequests { get; set; } = new List<AssetReturnRequest>();
        public ICollection<AssetReturnRequest> InitiatedReturnRequests { get; set; } = new List<AssetReturnRequest>();
        public ICollection<BrokenReport> BrokenReports { get; set; } = new List<BrokenReport>();
        public ICollection<AssetLiquidation> Liquidations { get; set; } = new List<AssetLiquidation>();
        public ICollection<AssetHistory> AssetHistories { get; set; } = new List<AssetHistory>();
        public ICollection<AssetHistory> ChangedAssetHistories { get; set; } = new List<AssetHistory>();
        public ICollection<AssetInspection> AssetInspections { get; set; } = new List<AssetInspection>();
        public ICollection<AssetReservation> AssetReservations { get; set; } = new List<AssetReservation>();
    }
}
