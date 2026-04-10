using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos
{
    public class AssetExportDto
    {
        public string AssetTag { get; set; }
        public string Category { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchaseValue { get; set; }
        public string Status { get; set; }

        public string DepartmentName { get; set; }
        public string UserEmail { get; set; }
    }
}
