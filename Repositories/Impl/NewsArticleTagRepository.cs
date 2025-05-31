using BusinessObject;

namespace Repositories.Impl
{
    public class NewsArticleTagRepository(FUNewsDbContext context)
        : GenericRepository<NewsArticleTag>(context) { }
}
