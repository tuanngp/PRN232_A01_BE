using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Auth;
using Services.Interface;

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

                _authService.SetTokenCookie(
                    "accessToken",
                    result.AccessToken,
                    result.AccessTokenExpires,
                    Response
                );
                _authService.SetTokenCookie(
                    "refreshToken",
                    result.RefreshToken,
                    result.RefreshTokenExpires,
                    Response
                );

                _logger.LogInformation("User {Email} logged in successfully", request.Email);

                return Success(result.User, "Đăng nhập thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user {Email}", request.Email);
                return Error("Có lỗi xảy ra trong quá trình đăng nhập.", 500);
            }
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
                    _authService.ClearTokenCookies(Response);
                    return Unauthorized("Refresh token không hợp lệ hoặc đã hết hạn.");
                }

                _authService.SetTokenCookie(
                    "accessToken",
                    result.AccessToken,
                    result.AccessTokenExpires,
                    Response
                );
                _authService.SetTokenCookie(
                    "refreshToken",
                    result.RefreshToken,
                    result.RefreshTokenExpires,
                    Response
                );

                return Success(result.User, "Refresh token thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token refresh");
                _authService.ClearTokenCookies(Response);
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

                _authService.SetTokenCookie(
                    "accessToken",
                    result.AccessToken,
                    result.AccessTokenExpires,
                    Response
                );
                _authService.SetTokenCookie(
                    "refreshToken",
                    result.RefreshToken,
                    result.RefreshTokenExpires,
                    Response
                );
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

                _authService.ClearTokenCookies(Response);

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
                            _authService.SetTokenCookie(
                                "accessToken",
                                refreshResult.AccessToken,
                                refreshResult.AccessTokenExpires,
                                Response
                            );
                            _authService.SetTokenCookie(
                                "refreshToken",
                                refreshResult.RefreshToken,
                                refreshResult.RefreshTokenExpires,
                                Response
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

                    _authService.ClearTokenCookies(Response);
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
    }
}
