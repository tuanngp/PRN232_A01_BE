using BusinessObject;
using BusinessObject.Enums;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Interface;

namespace Services.Impl
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly ILogger<NewsArticleService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public NewsArticleService(
            INewsArticleRepository newsArticleRepository,
            ILogger<NewsArticleService> logger,
            IUnitOfWork unitOfWork
        )
        {
            _newsArticleRepository = newsArticleRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<NewsArticle> AddAsync(NewsArticle entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                ValidateNewsArticle(entity);

                entity.CreatedDate = DateTime.UtcNow;
                entity.NewsStatus = NewsStatus.Inactive;

                await _newsArticleRepository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Created new article with ID: {NewsArticleId}",
                    entity.NewsArticleId
                );
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating news article: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(object id)
        {
            try
            {
                var article = await _newsArticleRepository.GetByIdAsync(id);
                if (article == null)
                    return false;

                article.IsDeleted = true;
                article.DeletedAt = DateTime.UtcNow;

                _newsArticleRepository.Update(article);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Soft deleted article with ID: {NewsArticleId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting news article: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticle>> GetAllAsync()
        {
            try
            {
                return await _newsArticleRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all news articles: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<NewsArticle?> GetByIdAsync(object id)
        {
            try
            {
                return await _newsArticleRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting news article by ID {Id}: {Message}",
                    id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByStatusAsync(NewsStatus status)
        {
            try
            {
                return await _newsArticleRepository.GetNewsByStatusAsync(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting news articles by status {Status}: {Message}",
                    status,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticle>> GetNewsByCategoryAsync(int categoryId)
        {
            try
            {
                return await _newsArticleRepository.GetNewsByCategoryAsync(categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting news articles by category {CategoryId}: {Message}",
                    categoryId,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<NewsArticle> UpdateAsync(NewsArticle entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingArticle = await _newsArticleRepository.GetByIdAsync(
                    entity.NewsArticleId
                );
                if (existingArticle == null)
                    throw new InvalidOperationException(
                        $"News article with ID {entity.NewsArticleId} not found"
                    );

                ValidateNewsArticle(entity);

                entity.ModifiedDate = DateTime.UtcNow;

                _newsArticleRepository.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated news article with ID: {NewsArticleId}",
                    entity.NewsArticleId
                );
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating news article: {Message}", ex.Message);
                throw;
            }
        }

        private void ValidateNewsArticle(NewsArticle entity)
        {
            if (string.IsNullOrEmpty(entity.NewsTitle))
                throw new ArgumentException("News title is required");

            if (entity.NewsTitle.Length > 200)
                throw new ArgumentException("News title cannot exceed 200 characters");

            if (string.IsNullOrEmpty(entity.NewsContent))
                throw new ArgumentException("News content is required");

            if (entity.Headline?.Length > 500)
                throw new ArgumentException("Headline cannot exceed 500 characters");

            if (entity.NewsSource?.Length > 200)
                throw new ArgumentException("News source cannot exceed 200 characters");

            if (entity.CategoryId <= 0)
                throw new ArgumentException("Invalid category ID");

            if (entity.CreatedById <= 0)
                throw new ArgumentException("Invalid creator ID");
        }
    }
}
