using AssetManagement.Application.dtos.Request.Authenication;
using AssetManagement.Application.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Server.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController(AuthenicationService _authService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await _authService.Login(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // No server-side token store to invalidate; endpoint exists so audit filter
            // can record a LOGOUT action when clients call it.
            return Ok(new { message = "Logout recorded" });
        }

        [HttpPost("create")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _authService.CreateUser(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var response = await _authService.ForgotPassword(request);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var response = await _authService.ResetPassword(request);
            return StatusCode(response.StatusCode, response);
        }
    }
}