using AssetManagement.Application.dtos.Request.AssetRequests;
using AssetManagement.Application.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/manager/approvals")]
    [Authorize(Roles = "MANAGER")]
    public class ManagerApprovalsController : ControllerBase
    {
        private readonly ManagerApprovalService _managerApprovalService;

        public ManagerApprovalsController(ManagerApprovalService managerApprovalService)
        {
            _managerApprovalService = managerApprovalService;
        }

        [HttpGet]
        public async Task<IActionResult> GetApprovalRequests()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdValue, out var managerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _managerApprovalService.GetApprovalRequestsAsync(managerId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdValue, out var managerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _managerApprovalService.ApproveRequestAsync(managerId, id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectAssetRequestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdValue, out var managerId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _managerApprovalService.RejectRequestAsync(managerId, id, request);
            return StatusCode(response.StatusCode, response);
        }
    }
}