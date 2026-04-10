using AssetManagement.Application.dtos.Request.Department;
using AssetManagement.Application.interfaces.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentsController(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var departments = await _departmentRepository.GetAllAsync(includeInactive);
            return Ok(departments);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return NotFound(new { message = "Department not found." });
            return Ok(department);
        }

        [HttpGet("{id:guid}/detail")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var department = await _departmentRepository.GetDetailsAsync(id);
            if (department == null) return NotFound(new { message = "Department not found." });
            return Ok(department);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _departmentRepository.ExistsByNameAsync(request.Name)) return BadRequest(new { message = "Department name already exists." });
            if (await _departmentRepository.ExistsByCodeAsync(request.Code)) return BadRequest(new { message = "Department code already exists." });
            if (request.ManagerId.HasValue && !await _departmentRepository.ManagerExistsAsync(request.ManagerId.Value)) return BadRequest(new { message = "Manager does not exist." });

            var department = await _departmentRepository.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var existing = await _departmentRepository.GetByIdAsync(id);
            if (existing == null) return NotFound(new { message = "Department not found." });
            if (await _departmentRepository.ExistsByNameAsync(request.Name, id)) return BadRequest(new { message = "Department name already exists." });
            if (await _departmentRepository.ExistsByCodeAsync(request.Code, id)) return BadRequest(new { message = "Department code already exists." });
            if (request.ManagerId.HasValue && !await _departmentRepository.ManagerExistsAsync(request.ManagerId.Value)) return BadRequest(new { message = "Manager does not exist." });

            var updated = await _departmentRepository.UpdateAsync(id, request);
            return Ok(updated);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDepartmentStatusRequest request)
        {
            var success = await _departmentRepository.SetStatusAsync(id, request.IsActive);
            if (!success) return NotFound(new { message = "Department not found." });
            return Ok(new { message = "Department status updated successfully." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _departmentRepository.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Department not found." });
            return NoContent();
        }
    }

    public class UpdateDepartmentStatusRequest
    {
        public bool IsActive { get; set; }
    }
}