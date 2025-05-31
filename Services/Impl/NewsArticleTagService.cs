using BusinessObject;
using Repositories.Impl;

namespace Services.Impl
{
    public class NewsArticleTagService : INewsArticleTagService
    {
        private readonly NewsArticleTagRepository _repository;

        public NewsArticleTagService(NewsArticleTagRepository repository)
        {
            _repository = repository;
        }

        public async Task<NewsArticleTag> AddAsync(NewsArticleTag entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<NewsArticleTag>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<NewsArticleTag?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<NewsArticleTag> UpdateAsync(NewsArticleTag entity)
        {
            return await _repository.UpdateAsync(entity);
        }
    }
}
