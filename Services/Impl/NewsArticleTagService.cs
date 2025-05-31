using BusinessObject;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Interface;

namespace Services.Impl
{
    public class NewsArticleTagService : INewsArticleTagService
    {
        private readonly INewsArticleTagRepository _newsArticleTagRepository;
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ILogger<NewsArticleTagService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public NewsArticleTagService(
            INewsArticleTagRepository newsArticleTagRepository,
            INewsArticleRepository newsArticleRepository,
            ITagRepository tagRepository,
            ILogger<NewsArticleTagService> logger,
            IUnitOfWork unitOfWork)
        {
            _newsArticleTagRepository = newsArticleTagRepository;
            _newsArticleRepository = newsArticleRepository;
            _tagRepository = tagRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<NewsArticleTag> AddAsync(NewsArticleTag entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                await ValidateNewsArticleTag(entity);

                await _newsArticleTagRepository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created new news article tag mapping: Article ID {ArticleId}, Tag ID {TagId}",
                    entity.NewsArticleId, entity.TagId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating news article tag mapping: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var mapping = await _newsArticleTagRepository.GetByIdAsync(id);
                if (mapping == null)
                    return false;

                _newsArticleTagRepository.Delete(mapping);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Deleted news article tag mapping: Article ID {ArticleId}, Tag ID {TagId}",
                    mapping.NewsArticleId, mapping.TagId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting news article tag mapping: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticleTag>> GetAllAsync()
        {
            try
            {
                return await _newsArticleTagRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all news article tag mappings: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<NewsArticleTag?> GetByIdAsync(int id)
        {
            try
            {
                return await _newsArticleTagRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting news article tag mapping by ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticleTag>> GetByNewsArticleIdAsync(int newsArticleId)
        {
            try
            {
                return await _newsArticleTagRepository.GetByNewsArticleIdAsync(newsArticleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting news article tag mappings by article ID {ArticleId}: {Message}",
                    newsArticleId, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticleTag>> GetByTagIdAsync(int tagId)
        {
            try
            {
                return await _newsArticleTagRepository.GetByTagIdAsync(tagId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting news article tag mappings by tag ID {TagId}: {Message}",
                    tagId, ex.Message);
                throw;
            }
        }

        public async Task<NewsArticleTag> UpdateAsync(NewsArticleTag entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingMapping = await _newsArticleTagRepository.GetByIdAsync(entity.NewsArticleId);
                if (existingMapping == null)
                    throw new InvalidOperationException($"News article tag mapping not found");

                await ValidateNewsArticleTag(entity);

                _newsArticleTagRepository.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated news article tag mapping: Article ID {ArticleId}, Tag ID {TagId}",
                    entity.NewsArticleId, entity.TagId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating news article tag mapping: {Message}", ex.Message);
                throw;
            }
        }

        private async Task ValidateNewsArticleTag(NewsArticleTag entity)
        {
            if (entity.NewsArticleId <= 0)
                throw new ArgumentException("Invalid news article ID");

            if (entity.TagId <= 0)
                throw new ArgumentException("Invalid tag ID");

            var newsArticle = await _newsArticleRepository.GetByIdAsync(entity.NewsArticleId);
            if (newsArticle == null)
                throw new InvalidOperationException($"News article with ID {entity.NewsArticleId} not found");

            if (newsArticle.IsDeleted)
                throw new InvalidOperationException("Cannot tag a deleted news article");

            var tag = await _tagRepository.GetByIdAsync(entity.TagId);
            if (tag == null)
                throw new InvalidOperationException($"Tag with ID {entity.TagId} not found");

            if (tag.IsDeleted)
                throw new InvalidOperationException("Cannot use a deleted tag");
        }
    }
}
