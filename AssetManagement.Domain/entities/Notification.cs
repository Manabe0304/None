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

    [Table("notifications")]
    public class Notification : BaseEntity
    {
        [Column("user_id")]
        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        [Column("title")]
        public string? Title { get; set; }

        [Column("message")]
        public string? Message { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; } = false;
    }
}
