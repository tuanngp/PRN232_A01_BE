using BusinessObject;

namespace Repositories.Impl
{
    public class TagRepository(FUNewsDbContext context) : GenericRepository<Tag>(context) { }
}
