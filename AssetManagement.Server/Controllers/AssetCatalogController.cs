using AssetManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/asset-catalog")]
    [Authorize]
    public class AssetCatalogController : ControllerBase
    {
        private readonly AssetDbContext _context;

        public AssetCatalogController(AssetDbContext context)
        {
            _context = context;
        }

        [HttpGet("models")]
        public async Task<IActionResult> GetModels([FromQuery] string assetType)
        {
            if (string.IsNullOrWhiteSpace(assetType))
                return Ok(new List<string>());

            var normalized = assetType.Trim().ToUpperInvariant();

            var models = await _context.Assets
                .AsNoTracking()
                .Where(a =>
                    !a.IsDeleted &&
                    a.Category != null &&
                    a.Category.ToUpper() == normalized &&
                    !string.IsNullOrWhiteSpace(a.Model))
                .Select(a => new { Model = a.Model!, AssetTag = a.AssetTag })
                .OrderBy(x => x.Model)
                .ThenBy(x => x.AssetTag)
                .ToListAsync();

            return Ok(new { data = models });
        }
    }
}