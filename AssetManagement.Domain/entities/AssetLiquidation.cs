using AssetManagement.Domain.Common;
using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("liquidations")]
    public class AssetLiquidation : BaseEntity
    {
        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("liquidation_reason")]
        public string LiquidationReason { get; set; } = string.Empty;

        [Column("liquidation_date")]
        public DateOnly LiquidationDate { get; set; }

        [Column("disposal_method")]
        public string? DisposalMethod { get; set; }

        [Column("liquidated_by")]
        public Guid LiquidatedById { get; set; }
        [ForeignKey(nameof(LiquidatedById))]
        public User LiquidatedBy { get; set; } = null!;

        [Column("notes")]
        public string? Notes { get; set; }
    }
}
