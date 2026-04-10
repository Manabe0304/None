using AssetManagement.Application.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/return-requests")]
    [Authorize(Roles = "EMPLOYEE")]
    public class ReturnRequestsController : ControllerBase
    {
        private readonly ReturnRequestService _returnRequestService;

        public ReturnRequestsController(ReturnRequestService returnRequestService)
        {
            _returnRequestService = returnRequestService;
        }

        [HttpGet("my-history")]
        public async Task<IActionResult> GetMyReturnedHistory()
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdValue, out var employeeId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _returnRequestService.GetMyReturnedHistoryAsync(employeeId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("my-history/{id:guid}")]
        public async Task<IActionResult> GetMyReturnedHistoryDetail(Guid id)
        {
            var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdValue, out var employeeId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var response = await _returnRequestService.GetMyReturnedHistoryDetailAsync(employeeId, id);
            return StatusCode(response.StatusCode, response);
        }
    }
}