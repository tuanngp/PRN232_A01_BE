using BusinessObject;
using BusinessObject.Enums;
using Repositories.Common;

namespace Repositories.Interface
{
    public interface INewsArticleRepository : IGenericRepository<NewsArticle>
    {
        Task<IEnumerable<NewsArticle>> GetNewsByStatusAsync(NewsStatus status);
        Task<IEnumerable<NewsArticle>> GetNewsByCategoryAsync(int categoryId);
    }
}
