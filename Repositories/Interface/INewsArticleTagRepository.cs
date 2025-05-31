using BusinessObject;
using Repositories.Common;

namespace Repositories.Interface
{
    public interface INewsArticleTagRepository : IGenericRepository<NewsArticleTag>
    {
        Task<IEnumerable<NewsArticleTag>> GetByNewsArticleIdAsync(int newsArticleId);
        Task<IEnumerable<NewsArticleTag>> GetByTagIdAsync(int tagId);
    }
}
