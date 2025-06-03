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
        private readonly IConfiguration _configuration;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger,
            IConfiguration configuration
        )
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
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

                SetTokenCookie("accessToken", result.AccessToken, result.AccessTokenExpires);

                SetTokenCookie("refreshToken", result.RefreshToken, result.RefreshTokenExpires);

                _logger.LogInformation("User {Email} logged in successfully", request.Email);

                return Success(result.User, "Đăng nhập thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user {Email}", request.Email);
                return Error("Có lỗi xảy ra trong quá trình đăng nhập.", 500);
            }
        }

        private void SetTokenCookie(string v, string refreshToken, object refreshTokenExpires)
        {
            throw new NotImplementedException();
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var accessToken = Request.Cookies["accessToken"];
                var refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(
                        "Access token hoặc Refresh token không tìm thấy trong cookie."
                    );
                }

                var refreshRequest = new RefreshTokenRequest
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                };
                var result = await _authService.RefreshTokenAsync(refreshRequest);

                if (result == null)
                {
                    ClearTokenCookies();
                    return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
                }

                SetTokenCookie("accessToken", result.AccessToken, result.AccessTokenExpires);
                SetTokenCookie("refreshToken", result.RefreshToken, result.RefreshTokenExpires);

                return Success(result.User, "Refresh token thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                ClearTokenCookies();
                return Error("Có lỗi xảy ra trong quá trình refresh token.", 500);
            }
        }

        [HttpPost("refresh-token-manual")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshTokenManual([FromBody] RefreshTokenRequest request)
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

                SetTokenCookie("accessToken", result.AccessToken, result.AccessTokenExpires);
                SetTokenCookie("refreshToken", result.RefreshToken, result.RefreshTokenExpires);

                return Success(result, "Refresh token thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during manual token refresh");
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
                var token =
                    Request.Cookies["accessToken"]
                    ?? Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    return ValidationError(
                        new { Token = "Token không được tìm thấy." },
                        "Token không hợp lệ."
                    );
                }

                var isValid = await _authService.ValidateTokenAsync(token);

                if (!isValid)
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                var (userId, email, role) = GetCurrentUser();

                return Success(
                    new
                    {
                        userId,
                        email,
                        role,
                        isValid = true,
                        tokenSource = Request.Cookies["accessToken"] != null ? "cookie" : "header",
                    },
                    "Token hợp lệ."
                );
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

                ClearTokenCookies();

                _logger.LogInformation("User {Email} logged out", email);

                return Success("Đăng xuất thành công. Token đã được xóa khỏi cookie.");
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

                return Success(
                    new
                    {
                        userId,
                        email,
                        role,
                    },
                    "Lấy thông tin profile thành công."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting profile");
                return Error("Có lỗi xảy ra khi lấy thông tin profile.", 500);
            }
        }

        [HttpGet("check-auth")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckAuth()
        {
            try
            {
                var accessToken = Request.Cookies["accessToken"];

                if (string.IsNullOrEmpty(accessToken))
                {
                    return Success(new { isAuthenticated = false }, "Chưa đăng nhập.");
                }

                var isValid = await _authService.ValidateTokenAsync(accessToken);

                if (!isValid)
                {
                    var refreshToken = Request.Cookies["refreshToken"];
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var refreshRequest = new RefreshTokenRequest
                        {
                            RefreshToken = refreshToken,
                        };
                        var refreshResult = await _authService.RefreshTokenAsync(refreshRequest);

                        if (refreshResult != null)
                        {
                            SetTokenCookie(
                                "accessToken",
                                refreshResult.AccessToken,
                                refreshResult.AccessTokenExpires
                            );
                            SetTokenCookie(
                                "refreshToken",
                                refreshResult.RefreshToken,
                                refreshResult.RefreshTokenExpires
                            );

                            return Success(
                                new
                                {
                                    isAuthenticated = true,
                                    user = refreshResult.User,
                                    refreshed = true,
                                },
                                "Token đã được refresh tự động."
                            );
                        }
                    }

                    ClearTokenCookies();
                    return Success(new { isAuthenticated = false }, "Token hết hạn.");
                }

                return Success(new { isAuthenticated = true }, "Đã đăng nhập.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking authentication");
                return Success(
                    new { isAuthenticated = false },
                    "Lỗi khi kiểm tra trạng thái đăng nhập."
                );
            }
        }

        private void SetTokenCookie(string cookieName, string token, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Ngăn chặn truy cập từ JavaScript
                Secure = true, // Chỉ gửi qua HTTPS (trong production)
                SameSite = SameSiteMode.Strict, // Bảo vệ CSRF
                Expires = expires,
                Path = "/",
                Domain = _configuration["Cookie:Domain"], // Có thể config domain
            };

            // Trong development, có thể tắt Secure để test với HTTP
            if (_configuration.GetValue<bool>("Development:AllowInsecureCookies"))
            {
                cookieOptions.Secure = false;
            }

            Response.Cookies.Append(cookieName, token, cookieOptions);
        }

        private void ClearTokenCookies()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/",
                Domain = _configuration["Cookie:Domain"],
            };

            if (_configuration.GetValue<bool>("Development:AllowInsecureCookies"))
            {
                cookieOptions.Secure = false;
            }

            Response.Cookies.Append("accessToken", "", cookieOptions);
            Response.Cookies.Append("refreshToken", "", cookieOptions);
        }
    }
}
