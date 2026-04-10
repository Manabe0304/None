using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Enums;
using AssetManagement.Infrastructure.Persistence;
using AssetManagement.Infrastructure.repositories;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly PieChart _repo;

        public DashboardController(PieChart repo)
        {
            _repo = repo;
        }

        [HttpGet("asset-status")]
        public async Task<IActionResult> GetAssetStatus() => Ok(await _repo.GetAssetStatusSummary());

        [HttpGet("repair-spending")]
        public async Task<IActionResult> GetRepairSpending() => Ok(await _repo.GetRepairSpendingByMonth());

        [HttpGet("kpi")]
        public async Task<IActionResult> GetKPI() => Ok(await _repo.GetDashboardKPI());

        [HttpGet("activity-trend")]
        public async Task<IActionResult> GetActivityTrend() => Ok(await _repo.GetAssetActivityTrend());

        [HttpGet("assets-by-department")]
        public async Task<IActionResult> GetAssetsByDepartment() => Ok(await _repo.GetAssetsByDepartment());

        [HttpGet("top-assets")]
        public async Task<IActionResult> GetTopAssets() => Ok(await _repo.GetTopUsedAssets());
    }

    [Route("api/assets")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetRepository _repo;

        public AssetsController(IAssetRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, string? search = null)
        {
            var result = await _repo.GetPagedAsync(page, search);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var asset = await _repo.GetByIdAsync(id);
            if (asset == null || asset.IsDeleted) return NotFound();
            return Ok(asset);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssetCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var normalized = NormalizeAssignment(dto.AssignmentType, dto.DepartmentId, dto.CurrentUserId, dto.Status);

            var asset = new Asset
            {
                Id = Guid.NewGuid(),
                AssetTag = dto.AssetTag,
                Category = dto.Category,
                Model = dto.Model,
                SerialNumber = dto.SerialNumber,
                PurchaseDate = DateTime.SpecifyKind(dto.PurchaseDate, DateTimeKind.Utc),
                PurchaseValue = dto.PurchaseValue,
                Status = normalized.Status,
                DepartmentId = normalized.DepartmentId,
                CurrentUserId = normalized.CurrentUserId,
                CreatedAt = DateTime.UtcNow,
            };

            await _repo.AddAsync(asset);
            return CreatedAtAction(nameof(GetById), new { id = asset.Id }, asset);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] AssetCreateUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var asset = await _repo.GetByIdAsync(id);
            if (asset == null || asset.IsDeleted) return NotFound();

            var normalized = NormalizeAssignment(dto.AssignmentType, dto.DepartmentId, dto.CurrentUserId, dto.Status);

            asset.AssetTag = dto.AssetTag;
            asset.Category = dto.Category;
            asset.Model = dto.Model;
            asset.SerialNumber = dto.SerialNumber;
            asset.PurchaseDate = DateTime.SpecifyKind(dto.PurchaseDate, DateTimeKind.Utc);
            asset.PurchaseValue = dto.PurchaseValue;
            asset.Status = normalized.Status;
            asset.DepartmentId = normalized.DepartmentId;
            asset.CurrentUserId = normalized.CurrentUserId;
            asset.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(asset);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var asset = await _repo.GetByIdAsync(id);
            if (asset == null || asset.IsDeleted) return NotFound();
            await _repo.SoftDeleteAsync(id);
            return NoContent();
        }

        [HttpPut("restore/{id:guid}")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var asset = await _repo.GetByIdAsync(id);
            if (asset == null || !asset.IsDeleted) return NotFound();
            await _repo.RestoreAsync(id);
            return NoContent();
        }

        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted(int page = 1, string? search = null)
        {
            var result = await _repo.GetDeletedAsync(page, search);
            return Ok(result);
        }

        private static (string Status, Guid? DepartmentId, Guid? CurrentUserId) NormalizeAssignment(string? assignmentType, Guid? departmentId, Guid? currentUserId, string? currentStatus)
        {
            var type = (assignmentType ?? "UNASSIGNED").Trim().ToUpperInvariant();
            if (type == "DEPARTMENT")
            {
                return (AssetStatus.IN_USE_SHARED.ToString(), departmentId, null);
            }

            if (type == "PERSONAL")
            {
                return (AssetStatus.IN_USE.ToString(), departmentId, currentUserId);
            }

            return (string.IsNullOrWhiteSpace(currentStatus) ? AssetStatus.AVAILABLE.ToString() : currentStatus!, departmentId, null);
        }
    }

    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly AssetDbContext _context;

        public TestController(AssetDbContext context)
        {
            _context = context;
        }

        [HttpGet("db")]
        public async Task<IActionResult> TestDb()
        {
            var canConnect = await _context.Database.CanConnectAsync();
            if (canConnect) return Ok("Database connected successfully");
            return StatusCode(500, "Cannot connect to database");
        }
    }
}