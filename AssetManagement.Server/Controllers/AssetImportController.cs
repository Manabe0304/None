using AssetManagement.Application.interfaces.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers
{
  
        [ApiController]
        [Route("api/admin/assets")]
        public class AssetImportController : ControllerBase
        {
            private readonly IAssetImportRepository _repo;

            public AssetImportController(IAssetImportRepository repo)
            {
                _repo = repo;
               
            }

            [HttpPost("import")]
            public async Task<IActionResult> Import(IFormFile file)
            {
                if (file == null || file.Length == 0)
                    return BadRequest("File is empty");

                var count = await _repo.ImportExcelAsync(file);

                return Ok(new
                {
                    message = $"Imported {count} assets successfully"
                });
            }

        [HttpGet("export")]
        public async Task<IActionResult> Export()
        {
            var file = await _repo.ExportExcelAsync();

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "assets.xlsx"
            );
        }
    }
    
}

