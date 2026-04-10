using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos
{
    public class MaintenanceRecordDto
    {
        public Guid Id { get; set; }
        public string AssetTag { get; set; }
        public string VendorName { get; set; }
        public string Status { get; set; }
        public string Outcome { get; set; }
        public decimal? EstimatedCost { get; set; }
    }
}
