using BusinessObject;
using Repositories.Common;

namespace Repositories.Interface
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetByAccountIdAsync(int accountId);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByAccountIdAsync(int accountId);
        Task RevokeAllTokensForUserAsync(int accountId, string reason);
    }
} 