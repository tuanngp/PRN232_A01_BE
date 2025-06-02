using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Auth;
using Services.Models.Auth;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState, "Thông tin đăng nhập không hợp lệ.");
                }

                var result = await _authService.LoginAsync(request);

                if (result == null)
                {
                    return Unauthorized("Email hoặc mật khẩu không chính xác.");
                }

                _logger.LogInformation("User {Email} logged in successfully", request.Email);

                return Success(result, "Đăng nhập thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user {Email}", request.Email);
                return Error("Có lỗi xảy ra trong quá trình đăng nhập.", 500);
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState, "Thông tin refresh token không hợp lệ.");
                }

                var result = await _authService.RefreshTokenAsync(request);

                if (result == null)
                {
                    return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
                }

                return Success(result, "Refresh token thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                return Error("Có lỗi xảy ra trong quá trình refresh token.", 500);
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState, "Thông tin token không hợp lệ.");
                }

                var (userId, _, _) = GetCurrentUser();
                
                if (!userId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng hiện tại.");
                }

                var result = await _authService.RevokeTokenAsync(request, userId.Value);

                if (!result)
                {
                    return NotFound("Token không tồn tại hoặc không thuộc về người dùng hiện tại.");
                }

                return Success("Thu hồi token thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token revocation");
                return Error("Có lỗi xảy ra trong quá trình thu hồi token.", 500);
            }
        }

        [HttpGet("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateToken()
        {
            try
            {
                var token = Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    return ValidationError(new { Token = "Token không được tìm thấy." }, "Token không hợp lệ.");
                }

                var isValid = await _authService.ValidateTokenAsync(token);

                if (!isValid)
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                var (userId, email, role) = GetCurrentUser();

                return Success(new
                {
                    userId,
                    email,
                    role,
                    isValid = true
                }, "Token hợp lệ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token validation");
                return Error("Có lỗi xảy ra trong quá trình kiểm tra token.", 500);
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var (userId, email, _) = GetCurrentUser();
                
                if (!userId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng hiện tại.");
                }

                await _authService.RevokeAllUserTokensAsync(userId.Value, "User logged out");
                
                _logger.LogInformation("User {Email} logged out", email);

                return Success("Đăng xuất thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout");
                return Error("Có lỗi xảy ra trong quá trình đăng xuất.", 500);
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            try
            {
                var (userId, email, role) = GetCurrentUser();

                return Success(new
                {
                    userId,
                    email,
                    role
                }, "Lấy thông tin profile thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting profile");
                return Error("Có lỗi xảy ra khi lấy thông tin profile.", 500);
            }
        }
    }
} 