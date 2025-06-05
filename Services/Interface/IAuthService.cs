using BusinessObject;
using Microsoft.AspNetCore.Http;
using Services.DTOs;
using Services.DTOs.Auth;

namespace Services.Interface
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<LoginResponse?> GoogleLoginAsync(GoogleLoginRequest request);
        Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> RevokeTokenAsync(RevokeTokenRequest request, int? accountId = null);
        Task<bool> RevokeAllUserTokensAsync(int accountId, string reason = "User logged out");
        Task<string> GenerateTokenAsync(int accountId, string email, string role);
        Task<RefreshToken> GenerateRefreshTokenAsync(int accountId);
        Task<bool> ValidateTokenAsync(string token);

        // Cookie management methods
        void SetTokenCookie(
            string cookieName,
            string token,
            DateTime expires,
            HttpResponse response
        );
        void ClearTokenCookies(HttpResponse response);
        Task<LoginResponse?> Register(CreateSystemAccountDto createDto);
    }
}
