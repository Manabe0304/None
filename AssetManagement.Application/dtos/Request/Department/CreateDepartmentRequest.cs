using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.dtos.Request.Department
{
    public class CreateDepartmentRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid? ManagerId { get; set; }
    }
}