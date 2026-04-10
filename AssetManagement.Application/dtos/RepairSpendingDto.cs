using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos
{
    public class RepairSpendingDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Total { get; set; }
    }
}
