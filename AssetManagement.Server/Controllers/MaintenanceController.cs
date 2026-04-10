using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceRepository _repo;

        public MaintenanceController(IMaintenanceRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string search = "", int page = 1, int pageSize = 10)
        {
            var (data, total) = await _repo.GetAllAsync(search, page, pageSize);
            return Ok(new { data, total });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateMaintenanceRecordDto dto)
        {
            try
            {
                var result = await _repo.CreateAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vendors")]
        public async Task<IActionResult> Vendors()
        {
            return Ok(await _repo.GetVendorsAsync());
        }

        [HttpGet("assets")]
        public async Task<IActionResult> Assets()
        {
            return Ok(await _repo.GetAssetsAsync());
        }
    }
}