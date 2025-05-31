using BusinessObject;
using Repositories.Impl;

namespace Services.Impl
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly NewsArticleRepository _repository;

        public NewsArticleService(NewsArticleRepository repository)
        {
            _repository = repository;
        }

        public async Task<NewsArticle> AddAsync(NewsArticle entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<NewsArticle>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<NewsArticle?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<NewsArticle> UpdateAsync(NewsArticle entity)
        {
            return await _repository.UpdateAsync(entity);
        }
    }
}
