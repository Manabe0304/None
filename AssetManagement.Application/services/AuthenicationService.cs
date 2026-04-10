using AssetManagement.Application.dtos.Request.Authenication;
using AssetManagement.Application.dtos.Response;
using AssetManagement.Application.dtos.Response.Auth;
using AssetManagement.Application.interfaces.IRepository;
using AssetManagement.Application.Services;
using BCrypt.Net;

namespace AssetManagement.Application.services
{
    public class AuthenicationService(IAuthenicationRepository _authRepository, JWTService _jwtService)
    {
        public async Task<ApiResponse<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _authRepository.GetByEmailAsync(request.Email?.Trim() ?? string.Empty);
            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return new ApiResponse<LoginResponse>
                {
                    StatusCode = 401,
                    Message = "Invalid email or password",
                    Data = null
                };
            }

            var isValidPassword = await VerifyAndUpgradePasswordAsync(user, request.Password);
            if (!isValidPassword)
            {
                return new ApiResponse<LoginResponse>
                {
                    StatusCode = 401,
                    Message = "Invalid email or password",
                    Data = null
                };
            }

            string token = _jwtService.GenerateToken(user);

            var response = new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role?.Name,
                DepartmentName = user.Department?.Name,
                Token = token
            };

            return new ApiResponse<LoginResponse>
            {
                StatusCode = 200,
                Message = "Login successful",
                Data = response
            };
        }

        private async Task<bool> VerifyAndUpgradePasswordAsync(Domain.Entities.User user, string plainPassword)
        {
            var stored = user.PasswordHash?.Trim();
            if (string.IsNullOrWhiteSpace(stored) || string.IsNullOrWhiteSpace(plainPassword))
                return false;

            // Case 1: hash đúng chuẩn BCrypt
            if (LooksLikeBcryptHash(stored))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(plainPassword, stored);
                }
                catch
                {
                    return false;
                }
            }

            // Case 2: dữ liệu cũ đang lưu plaintext
            // Chỉ giữ fallback này tạm thời để migrate account cũ.
            if (stored == plainPassword)
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _authRepository.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private static bool LooksLikeBcryptHash(string value)
        {
            return value.StartsWith("$2a$") ||
                   value.StartsWith("$2b$") ||
                   value.StartsWith("$2y$");
        }

        public async Task<ApiResponse<Domain.Entities.User>> CreateUser(RegisterUserRequest request)
        {
            if (await _authRepository.ExistsByEmailAsync(request.Email))
            {
                return new ApiResponse<Domain.Entities.User>
                {
                    StatusCode = 400,
                    Message = "Email already exists"
                };
            }

            if (!await _authRepository.RoleExistsAsync(request.RoleId))
            {
                return new ApiResponse<Domain.Entities.User>
                {
                    StatusCode = 404,
                    Message = "Role does not exist"
                };
            }

            if (request.DepartmentId.HasValue && request.DepartmentId.Value != Guid.Empty)
            {
                if (!await _authRepository.DepartmentExistsAsync(request.DepartmentId.Value))
                {
                    return new ApiResponse<Domain.Entities.User>
                    {
                        StatusCode = 404,
                        Message = "Department does not exist"
                    };
                }
            }

            var newUser = new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                RoleId = request.RoleId,
                DepartmentId = request.DepartmentId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            var createdUser = await _authRepository.CreateUserAsync(newUser);

            return new ApiResponse<Domain.Entities.User>
            {
                StatusCode = 201,
                Message = "User created successfully",
                Data = createdUser
            };
        }

        public async Task<ApiResponse<object>> ForgotPassword(ForgotPasswordRequest request)
        {
            var user = await _authRepository.GetByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                return new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = "Email does not exist",
                    Data = null
                };
            }

            return new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Email verified",
                Data = new
                {
                    email = user.Email,
                    userId = user.Id
                }
            };
        }

        public async Task<ApiResponse<object>> ResetPassword(ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return new ApiResponse<object>
                {
                    StatusCode = 400,
                    Message = "Password confirmation does not match",
                    Data = null
                };
            }

            var user = await _authRepository.GetByEmailAsync(request.Email.Trim());
            if (user == null)
            {
                return new ApiResponse<object>
                {
                    StatusCode = 404,
                    Message = "Account not found",
                    Data = null
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _authRepository.SaveChangesAsync();

            return new ApiResponse<object>
            {
                StatusCode = 200,
                Message = "Password updated successfully",
                Data = new { email = user.Email }
            };
        }
    }
}