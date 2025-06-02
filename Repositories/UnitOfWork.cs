using Microsoft.EntityFrameworkCore.Storage;
using Repositories.Common;
using Repositories.Interface;

namespace Repositories
{
    public class UnitOfWork(
        FUNewsDbContext context,
        ICategoryRepository categoryRepository,
        INewsArticleRepository newsArticleRepository,
        INewsArticleTagRepository newsArticleTagRepository,
        ISystemAccountRepository systemAccountRepository,
        ITagRepository tagRepository,
        IRefreshTokenRepository refreshTokenRepository
    ) : IUnitOfWork, IDisposable
    {
        private readonly FUNewsDbContext _context = context;
        private IDbContextTransaction _transaction;
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();

        public ICategoryRepository Categories { get; } = categoryRepository;
        public INewsArticleRepository NewsArticles { get; } = newsArticleRepository;
        public INewsArticleTagRepository NewsArticleTags { get; } = newsArticleTagRepository;
        public ISystemAccountRepository SystemAccounts { get; } = systemAccountRepository;
        public ITagRepository Tags { get; } = tagRepository;
        public IRefreshTokenRepository RefreshTokens { get; } = refreshTokenRepository;

        public IGenericRepository<T> GetRepository<T>()
            where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                var repository = new GenericRepository<T>(_context);
                _repositories.Add(type, repository);
            }
            return (IGenericRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
