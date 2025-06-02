using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repositories.Common;
using Repositories.Interface;

namespace Repositories.Impl
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(FUNewsDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(r => r.Account)
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Where(r => r.AccountId == accountId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Where(r => r.AccountId == accountId && !r.IsUsed && !r.IsRevoked && r.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();
        }

        public async Task RevokeAllTokensForUserAsync(int accountId, string reason)
        {
            var activeTokens = await _dbSet
                .Where(r => r.AccountId == accountId && !r.IsUsed && !r.IsRevoked && r.ExpiryDate > DateTime.UtcNow)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedDate = DateTime.UtcNow;
                token.ReasonRevoked = reason;
            }

            _context.UpdateRange(activeTokens);
        }
    }
} 