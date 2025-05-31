using BusinessObject;

namespace Repositories.Impl
{
    public class SystemAccountRepository(FUNewsDbContext context)
        : GenericRepository<SystemAccount>(context) { }
}
