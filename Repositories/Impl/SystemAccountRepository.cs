using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Common;
using Repositories.Interface;

namespace Repositories.Impl
{
    public class SystemAccountRepository
        : GenericRepository<SystemAccount>,
            ISystemAccountRepository
    {
        public SystemAccountRepository(FUNewsDbContext context)
            : base(context) { }

        public async Task<SystemAccount> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(sa =>
                sa.AccountEmail.ToLower() == email.ToLower()
            );
        }

        public async Task<IEnumerable<SystemAccount>> GetByRoleAsync(AccountRole role)
        {
            return await _dbSet.Where(sa => sa.AccountRole == role).AsNoTracking().ToListAsync();
        }

        public async Task<bool> IsEmailExistAsync(string email)
        {
            return await _dbSet.AnyAsync(sa => sa.AccountEmail.ToLower() == email.ToLower());
        }
    }
}
