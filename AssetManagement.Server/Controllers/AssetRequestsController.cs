using AssetManagement.Application.dtos.Request.AssetRequests;
using AssetManagement.Application.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/asset-requests")]
    [Authorize(Roles = "EMPLOYEE")]
    public class AssetRequestsController : ControllerBase
    {
        private readonly AssetRequestService _assetRequestService;

        public AssetRequestsController(AssetRequestService assetRequestService)
        {
            _assetRequestService = assetRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] SubmitAssetRequestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdValue, out var employeeId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _assetRequestService.SubmitRequestAsync(employeeId, request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdValue, out var employeeId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _assetRequestService.GetMyRequestsAsync(employeeId);
            return StatusCode(response.StatusCode, response);
        }
    }
}