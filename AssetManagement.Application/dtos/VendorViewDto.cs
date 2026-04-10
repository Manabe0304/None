using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagement.Application.dtos
{
    public class VendorViewDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContactEmail { get; set; }
        public string Phone { get; set; }
    }
}
