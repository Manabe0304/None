using AssetManagement.Application.dtos;
using AssetManagement.Application.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/assets")]
    [Authorize(Roles = "ADMIN")]
    public class AssetLifecycleController : ControllerBase
    {
        private readonly IAssetLifecycleService _service;

        public AssetLifecycleController(IAssetLifecycleService service)
        {
            _service = service;
        }

        [HttpPost("{id:guid}/mark-beyond-repair")]
        public async Task<IActionResult> MarkBeyondRepair(Guid id, [FromBody] MarkAssetBeyondRepairRequest request)
        {
            try
            {
                var adminId = ResolveCurrentUserId();
                if (!adminId.HasValue)
                    return Unauthorized(new { message = "Invalid token." });

                var result = await _service.MarkBeyondRepairAsync(id, request, adminId.Value);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/liquidate")]
        public async Task<IActionResult> Liquidate(Guid id, [FromBody] LiquidateAssetRequest request)
        {
            try
            {
                var adminId = ResolveCurrentUserId();
                if (!adminId.HasValue)
                    return Unauthorized(new { message = "Invalid token." });

                var result = await _service.LiquidateAsync(id, request, adminId.Value);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        private Guid? ResolveCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var userId) ? userId : null;
        }
    }
}