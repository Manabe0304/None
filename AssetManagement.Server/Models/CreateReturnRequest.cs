using System;

namespace AssetManagement.Server.Models
{
    public class CreateReturnRequest
    {
        public Guid AssignmentId { get; set; }
        public Guid AssetId { get; set; }
    }
}
