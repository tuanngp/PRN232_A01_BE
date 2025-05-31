using BusinessObject;

namespace Repositories.Impl
{
    public class NewsArticleRepository(FUNewsDbContext context)
        : GenericRepository<NewsArticle>(context) { }
}
