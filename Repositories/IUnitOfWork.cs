using Repositories.Common;
using Repositories.Interface;

namespace Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> GetRepository<T>()
            where T : class;
        ICategoryRepository Categories { get; }
        INewsArticleRepository NewsArticles { get; }
        INewsArticleTagRepository NewsArticleTags { get; }
        ISystemAccountRepository SystemAccounts { get; }
        ITagRepository Tags { get; }
        IRefreshTokenRepository RefreshTokens { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
