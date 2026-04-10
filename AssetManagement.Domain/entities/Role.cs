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

    [Table("roles")]
    public class Role : BaseEntity
    {
        [Column("name")]
        public string Name { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
