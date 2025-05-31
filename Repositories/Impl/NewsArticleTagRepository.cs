using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repositories.Common;
using Repositories.Interface;

namespace Repositories.Impl
{
    public class NewsArticleTagRepository
        : GenericRepository<NewsArticleTag>,
            INewsArticleTagRepository
    {
        public NewsArticleTagRepository(FUNewsDbContext context)
            : base(context) { }

        public async Task<IEnumerable<NewsArticleTag>> GetByNewsArticleIdAsync(int newsArticleId)
        {
            return await _dbSet
                .Where(nat => nat.NewsArticleId == newsArticleId)
                .Include(nat => nat.Tag)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticleTag>> GetByTagIdAsync(int tagId)
        {
            return await _dbSet
                .Where(nat => nat.TagId == tagId)
                .Include(nat => nat.NewsArticle)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
