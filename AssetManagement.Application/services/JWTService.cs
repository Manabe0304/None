using AssetManagement.Domain.Entities;
using AssetManagement.Domain.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace AssetManagement.Application.Services
{
    public class JWTService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JWTService(IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor)
        {
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateToken(User user)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            string ip = "Unknown";
            string fingerprint = "Unknown";

            if (httpContext != null)
            {
                ip = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();

                if (!string.IsNullOrEmpty(ip))
                {
                    ip = ip.Split(',')[0].Trim();
                }
                else
                {
                    ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                }

                fingerprint = GenerateFingerprint(httpContext);
            }

            var claims = new List<Claim>
            {
                // 🔥 QUAN TRỌNG
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),

                // 🔥 QUAN TRỌNG
                new Claim(ClaimTypes.Email, user.Email ?? ""),

                // 🔥 FIX CHÍNH: ADD DEPARTMENT
                new Claim("department", user.Department?.Name ?? ""),

                // optional
                new Claim("ip", ip),
                new Claim("fp", fingerprint)
            };

            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));
            }

            if (string.IsNullOrEmpty(_jwtSettings.Key))
            {
                throw new ArgumentNullException("JWT Key is missing in configuration.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateFingerprint(HttpContext context)
        {
            var ua = context.Request.Headers["User-Agent"].ToString();
            var lang = context.Request.Headers["Accept-Language"].ToString();
            var platform = context.Request.Headers["Sec-CH-UA-Platform"].ToString();

            var raw = $"{ua}|{lang}|{platform}";
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(raw);

            return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
    }
}