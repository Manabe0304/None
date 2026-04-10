using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("asset_inspections")]
    public class AssetInspection
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("asset_id")]
        public Guid AssetId { get; set; }
        public Asset Asset { get; set; } = null!;

        [Column("return_request_id")]
        public Guid? ReturnRequestId { get; set; }
        public AssetReturnRequest? ReturnRequest { get; set; }

        [Column("inspected_by")]
        public Guid InspectedById { get; set; }

        [ForeignKey(nameof(InspectedById))]
        public User InspectedBy { get; set; } = null!;

        [Column("condition")]
        public string? Condition { get; set; }

        [Column("findings")]
        public string? Findings { get; set; }

        [Column("inspection_result")]
        public string? InspectionResult { get; set; }

        [Column("next_action")]
        public string? NextAction { get; set; }

        [Column("status")]
        public string Status { get; set; } = "PENDING";

        [Column("inspected_at")]
        public DateTime? InspectedAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
