using AssetManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.entities
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("vendor")]
    public class Vendor : BaseEntity
    {
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("contact_email")]
        public string? ContactEmail { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();
    }
}
