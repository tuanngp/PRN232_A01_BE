using BusinessObject;
using Repositories.Common;
using Repositories.Interface;

namespace Repositories.Impl
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(FUNewsDbContext context)
            : base(context) { }

        // Specific methods for Category can be added here if needed
    }
}
