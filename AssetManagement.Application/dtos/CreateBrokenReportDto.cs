using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos
{
    public class CreateBrokenReportDto
    {
        public Guid AssetId { get; set; }
        public string Description { get; set; }
    }
}
