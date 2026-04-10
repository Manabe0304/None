using AssetManagement.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetManagement.Domain.Entities
{
    [Table("departments")]
    public class Department : BaseEntity
    {
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("code")]
        public string Code { get; set; } = null!;

        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("manager_id")]
        public Guid? ManagerId { get; set; }

        public User? Manager { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}