using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos
{
    public class MyAssetDto
    {
        public Guid AssignmentId { get; set; }
        public Guid AssetId { get; set; }

        public string AssetTag { get; set; }
        public string Category { get; set; }
        public string Model { get; set; }

        public DateTime AssignedAt { get; set; }
    }
}
