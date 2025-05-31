using BusinessObject;
using BusinessObject.Enums;
using Repositories.Common;

namespace Repositories.Interface
{
    public interface ISystemAccountRepository : IGenericRepository<SystemAccount>
    {
        Task<SystemAccount> GetByEmailAsync(string email);
        Task<IEnumerable<SystemAccount>> GetByRoleAsync(AccountRole role);
        Task<bool> IsEmailExistAsync(string email);
    }
}
