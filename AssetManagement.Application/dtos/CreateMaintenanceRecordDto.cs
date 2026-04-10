using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos
{
    public class CreateMaintenanceRecordDto
    {
        public Guid AssetId { get; set; }
        public Guid? VendorId { get; set; }
        public string Description { get; set; }
        public decimal? EstimatedCost { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
