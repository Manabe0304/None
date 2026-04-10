using AssetManagement.Application.dtos.Request.User;
using AssetManagement.Application.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AssetManagementServer.Controllers

{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly AssetManagement.Infrastructure.Persistence.AssetDbContext _context;

        public UsersController(IUserService service, AssetManagement.Infrastructure.Persistence.AssetDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(int page = 1, string? search = null)
        {
            var result = await _service.GetUsersAsync(page, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _service.GetUserByIdAsync(id);

            if (user == null) return NotFound();

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (request.RoleId == null && !string.IsNullOrEmpty(request.RoleName))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
                if (role != null) request.RoleId = role.Id;
            }

            if (request.DepartmentId == null && !string.IsNullOrEmpty(request.DepartmentName))
            {
                var dep = await _context.Departments.FirstOrDefaultAsync(d => d.Name == request.DepartmentName);
                if (dep != null) request.DepartmentId = dep.Id;
            }

            await _service.CreateUserAsync(request);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                if (request.DepartmentId == Guid.Empty)
                {
                    request.DepartmentId = null;
                }

                if (request.RoleId == Guid.Empty)
                {
                    request.RoleId = null;
                }

                Console.WriteLine("UpdateUser payload: " + JsonSerializer.Serialize(request));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.InnerException?.Message);

                return BadRequest(new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }

            Guid? resolvedRoleId = null;
            Guid? resolvedDepartmentId = null;

            if (!string.IsNullOrEmpty(request.RoleName))
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.RoleName);
                if (role == null)
                {
                    return BadRequest(new { message = "Invalid role" });
                }
                resolvedRoleId = role.Id;
            }
            else if (request.RoleId.HasValue)
            {
                var role = await _context.Roles.FindAsync(request.RoleId.Value);
                if (role == null)
                {
                    return BadRequest(new { message = "Invalid role" });
                }
                resolvedRoleId = role.Id;
            }

            if (!string.IsNullOrEmpty(request.DepartmentName))
            {
                var dep = await _context.Departments.FirstOrDefaultAsync(d => d.Name == request.DepartmentName);
                if (dep == null)
                {
                    return BadRequest(new { message = "Invalid department" });
                }
                resolvedDepartmentId = dep.Id;
            }
            else if (request.DepartmentId.HasValue)
            {
                var dep = await _context.Departments.FindAsync(request.DepartmentId.Value);
                if (dep == null)
                {
                    return BadRequest(new { message = "Invalid department" });
                }
                resolvedDepartmentId = dep.Id;
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(request.Email);
                }
                catch
                {
                    return BadRequest(new { message = "Invalid email format" });
                }
            }

            request.RoleId = resolvedRoleId;
            request.DepartmentId = resolvedDepartmentId;

            try
            {
                await _service.UpdateUserAsync(id, request);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _service.DeleteUserAsync(id);
            return Ok();
        }
    }
}