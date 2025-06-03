using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessObject;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Interface;
using Services.DTOs.Auth;
using Services.Interface;
using Services.Util;

namespace Services.Impl
{
    public class AuthService : IAuthService
    {
        private readonly ISystemAccountService _systemAccountService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public AuthService(
            ISystemAccountService systemAccountService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IRefreshTokenRepository refreshTokenRepository
        )
        {
            _systemAccountService = systemAccountService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                var accounts = await _systemAccountService.GetAllAsync();
                var user = accounts.FirstOrDefault(x =>
                    x.AccountEmail.Equals(request.Email, StringComparison.OrdinalIgnoreCase)
                );

                if (user == null || !user.IsActive)
                {
                    throw new UnauthorizedAccessException(
                        "Email không tồn tại hoặc tài khoản đã bị vô hiệu hóa"
                    );
                }

                if (!PasswordUtil.VerifyPassword(request.Password, user.AccountPassword))
                {
                    throw new UnauthorizedAccessException("Mật khẩu không chính xác");
                }

                // Generate JWT token
                var token = await GenerateTokenAsync(
                    user.AccountId,
                    user.AccountEmail,
                    user.AccountRole.ToString()
                );

                // Generate refresh token
                var refreshToken = await GenerateRefreshTokenAsync(user.AccountId);

                // Lưu refresh token vào database
                await _unitOfWork.SaveChangesAsync();

                var accessDurationInMinutes = Convert.ToInt32(
                    _configuration.GetSection("Jwt")["DurationInMinutes"] ?? "60"
                );

                var refreshDurationInDays = Convert.ToInt32(
                    _configuration.GetSection("Jwt")["RefreshTokenDurationInDays"] ?? "7"
                );
                var accessTokenExpires = DateTime.UtcNow.AddMinutes(accessDurationInMinutes);
                var refreshTokenExpires = DateTime.UtcNow.AddDays(refreshDurationInDays);

                return new LoginResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken.Token,
                    AccessTokenExpires = accessTokenExpires,
                    RefreshTokenExpires = refreshTokenExpires,
                    User = new UserInfo
                    {
                        AccountId = user.AccountId,
                        AccountName = user.AccountName,
                        AccountEmail = user.AccountEmail,
                        AccountRole = user.AccountRole.ToString(),
                    },
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                // Validate access token (even if expired)
                var principal = GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null)
                {
                    throw new UnauthorizedAccessException("Access token không hợp lệ");
                }

                // Get user info from claims
                var accountIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (
                    string.IsNullOrEmpty(accountIdClaim)
                    || !int.TryParse(accountIdClaim, out var accountId)
                )
                {
                    throw new UnauthorizedAccessException(
                        "Token không chứa thông tin người dùng hợp lệ"
                    );
                }

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(role))
                {
                    throw new UnauthorizedAccessException("Token thiếu thông tin email hoặc role");
                }

                // Check if refresh token exists and is valid
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
                    request.RefreshToken
                );
                if (refreshToken == null)
                {
                    throw new UnauthorizedAccessException("Refresh token không tồn tại");
                }
                if (!refreshToken.IsActive)
                {
                    throw new UnauthorizedAccessException("Refresh token đã bị vô hiệu hóa");
                }
                if (refreshToken.AccountId != accountId)
                {
                    throw new UnauthorizedAccessException(
                        "Refresh token không thuộc về người dùng này"
                    );
                }

                // Mark current refresh token as used
                refreshToken.IsUsed = true;
                _unitOfWork.RefreshTokens.Update(refreshToken);

                // Generate new tokens
                var newAccessToken = await GenerateTokenAsync(accountId, email, role);
                var newRefreshToken = await GenerateRefreshTokenAsync(accountId);

                // Set replaced by token
                refreshToken.ReplacedByToken = newRefreshToken.Token;

                // Save changes
                await _unitOfWork.SaveChangesAsync();

                var accessDurationInMinutes = Convert.ToInt32(
                    _configuration.GetSection("Jwt")["DurationInMinutes"] ?? "60"
                );

                var refreshDurationInDays = Convert.ToInt32(
                    _configuration.GetSection("Jwt")["RefreshTokenDurationInDays"] ?? "7"
                );
                var accessTokenExpires = DateTime.UtcNow.AddMinutes(accessDurationInMinutes);
                var refreshTokenExpires = DateTime.UtcNow.AddDays(refreshDurationInDays);

                return new LoginResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    AccessTokenExpires = accessTokenExpires,
                    RefreshTokenExpires = refreshTokenExpires,
                    User = new UserInfo
                    {
                        AccountId = accountId,
                        AccountName = refreshToken.Account.AccountName,
                        AccountEmail = email,
                        AccountRole = role,
                    },
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(RevokeTokenRequest request, int? accountId = null)
        {
            try
            {
                var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
                    request.RefreshToken
                );
                if (refreshToken == null)
                {
                    throw new UnauthorizedAccessException("Refresh token không tồn tại");
                }

                // If accountId is provided, only allow revocation if the token belongs to the user
                if (accountId.HasValue && refreshToken.AccountId != accountId.Value)
                {
                    throw new UnauthorizedAccessException(
                        "Không có quyền thu hồi refresh token này"
                    );
                }

                // Revoke token
                refreshToken.IsRevoked = true;
                refreshToken.RevokedDate = DateTime.UtcNow;
                refreshToken.ReasonRevoked = "Revoked by user";

                _unitOfWork.RefreshTokens.Update(refreshToken);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Lỗi khi thu hồi token: {ex.Message}");
            }
        }

        public async Task<bool> RevokeAllUserTokensAsync(
            int accountId,
            string reason = "User logged out"
        )
        {
            try
            {
                await _refreshTokenRepository.RevokeAllTokensForUserAsync(accountId, reason);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GenerateTokenAsync(int accountId, string email, string role)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, accountId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64
                ),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(jwtSettings["DurationInMinutes"])
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RefreshToken> GenerateRefreshTokenAsync(int accountId)
        {
            // Generate a secure random token
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = Convert.ToBase64String(randomBytes);

            // Get refresh token expiry from config (default 7 days)
            var refreshTokenDurationInDays = 7; // Default value
            var durationString = _configuration.GetSection("Jwt:RefreshTokenDurationInDays").Value;
            if (
                !string.IsNullOrEmpty(durationString)
                && int.TryParse(durationString, out var configDuration)
            )
            {
                refreshTokenDurationInDays = configDuration;
            }

            // Create refresh token entity
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenDurationInDays),
                AccountId = accountId,
                CreatedDate = DateTime.UtcNow,
            };

            // Save to database
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            return refreshTokenEntity;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("Token không được để trống");
            }

            try
            {
                var jwtSettings = _configuration.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero,
                };

                var principal = tokenHandler.ValidateToken(
                    token,
                    validationParameters,
                    out var validatedToken
                );

                if (validatedToken == null)
                {
                    throw new UnauthorizedAccessException("Token không hợp lệ");
                }

                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                throw new UnauthorizedAccessException("Token đã hết hạn");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                throw new UnauthorizedAccessException("Chữ ký token không hợp lệ");
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Lỗi xác thực token: {ex.Message}");
            }
        }

        public void SetTokenCookie(
            string cookieName,
            string token,
            DateTime expires,
            HttpResponse response
        )
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires,
                Path = "/",
                Domain = _configuration["Cookie:Domain"],
            };

            if (_configuration.GetValue<bool>("Development:AllowInsecureCookies"))
            {
                cookieOptions.Secure = false;
            }

            response.Cookies.Append(cookieName, token, cookieOptions);
        }

        public void ClearTokenCookies(HttpResponse response)
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

            response.Cookies.Append("accessToken", "", cookieOptions);
            response.Cookies.Append("refreshToken", "", cookieOptions);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out var securityToken
                );

                if (
                    securityToken is not JwtSecurityToken jwtSecurityToken
                    || !jwtSecurityToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
