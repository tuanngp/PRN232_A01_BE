using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObject;
using BusinessObject.Enums;
using Microsoft.EntityFrameworkCore;
using Repositories.Common;
using Repositories.Interface;

namespace Repositories.Impl
{
    public class NewsArticleRepository : GenericRepository<NewsArticle>, INewsArticleRepository
    {
        public NewsArticleRepository(FUNewsDbContext context)
            : base(context) { }

        public async Task<IEnumerable<NewsArticle>> GetNewsByStatusAsync(NewsStatus status)
        {
            return await _dbSet.Where(n => n.NewsStatus == status).AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByCategoryAsync(int categoryId)
        {
            return await _dbSet.Where(n => n.CategoryId == categoryId).AsNoTracking().ToListAsync();
        }
    }
}
