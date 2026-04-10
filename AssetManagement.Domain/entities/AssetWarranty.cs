using AssetManagement.Domain.Common;
using AssetManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Domain.entities
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("asset_warranties")]
    public class AssetWarranty : BaseEntity
    {
        [Column("asset_id")]
        public Guid AssetId { get; set; }

        public Asset Asset { get; set; } = null!;

        [Column("provider")]
        public string? Provider { get; set; }

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }
    }
}
