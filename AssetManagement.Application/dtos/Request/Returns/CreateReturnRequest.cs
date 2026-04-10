using System;

namespace AssetManagement.Application.dtos.Request.Returns
{
    public class CreateReturnRequest
    {
        public Guid AssignmentId { get; set; }
        public Guid AssetId { get; set; }
    }
}
