using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/vendors")]
    public class VendorController : ControllerBase
    {
        private readonly IVendorRepository _repo;

        public VendorController(IVendorRepository repo)
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
        public async Task<IActionResult> Create(VendorDto dto)
        {
            return Ok(await _repo.CreateAsync(dto));
        }

        [HttpPut]
        public async Task<IActionResult> Update(VendorDto dto)
        {
            return Ok(await _repo.UpdateAsync(dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await _repo.DeleteAsync(id));
        }
    }
}
