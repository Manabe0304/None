using AssetManagement.Domain.Common;
using AssetManagement.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.entities
{
    [Table("import_batches")]
    public class ImportBatch : BaseEntity
    {
        [Column("file_name")]
        public string? FileName { get; set; }

        [Column("imported_by")]
        public Guid? ImportedBy { get; set; }

        [ForeignKey(nameof(ImportedBy))]
        public User? ImportedByUser { get; set; }

        [Column("user_id")]
        public Guid? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Column("total_records")]
        public int TotalRecords { get; set; }
    }
}
