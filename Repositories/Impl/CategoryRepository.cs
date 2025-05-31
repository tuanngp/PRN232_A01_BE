using BusinessObject;

namespace Repositories.Impl
{
    public class CategoryRepository(FUNewsDbContext context)
        : GenericRepository<Category>(context) { }
}
