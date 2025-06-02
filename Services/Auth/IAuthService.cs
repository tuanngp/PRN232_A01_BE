using BusinessObject;
using Services.Models.Auth;

namespace Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<LoginResponse?> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> RevokeTokenAsync(RevokeTokenRequest request, int? accountId = null);
        Task<bool> RevokeAllUserTokensAsync(int accountId, string reason = "User logged out");
        Task<string> GenerateTokenAsync(int accountId, string email, string role);
        Task<RefreshToken> GenerateRefreshTokenAsync(int accountId);
        Task<bool> ValidateTokenAsync(string token);
    }
}
