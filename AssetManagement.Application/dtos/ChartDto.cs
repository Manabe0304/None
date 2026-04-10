using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos
{
    public  class ChartDto
    {

        public class AssetStatusDto
        {
            public string Status { get; set; }
            public int Count { get; set; }
            public double Percentage { get; set; }
        }
    }
}
