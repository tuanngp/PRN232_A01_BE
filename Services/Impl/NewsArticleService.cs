using BusinessObject;
using BusinessObject.Enums;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.DTOs;
using Services.Interface;

namespace Services.Impl
{
    public class NewsArticleService : INewsArticleService
    {
        private readonly ILogger<NewsArticleService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public NewsArticleService(ILogger<NewsArticleService> logger, IUnitOfWork unitOfWork)
        {
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

                await _unitOfWork.NewsArticles.AddAsync(entity);
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

        public async Task<NewsArticleDto> CreateNewsArticleAsync(
            CreateNewsArticleDto createDto,
            int currentUserId
        )
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(createDto.CategoryId);
                if (category == null)
                {
                    throw new ArgumentException("Danh mục không tồn tại");
                }

                var article = new NewsArticle
                {
                    NewsTitle = createDto.NewsTitle,
                    Headline = createDto.Headline,
                    NewsContent = createDto.NewsContent,
                    NewsSource = createDto.NewsSource,
                    ImageUrl = createDto.ImageUrl,
                    CategoryId = createDto.CategoryId,
                    CreatedById = currentUserId,
                    CreatedDate = DateTime.UtcNow,
                    NewsStatus = NewsStatus.Inactive,
                };

                var createdArticle = await AddAsync(article);
                return MapToDto(createdArticle);
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
                var article = await _unitOfWork.NewsArticles.GetByIdAsync(id);
                if (article == null)
                    return false;

                article.IsDeleted = true;
                article.DeletedAt = DateTime.UtcNow;

                _unitOfWork.NewsArticles.Update(article);
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

        public async Task<bool> DeleteNewsArticleAsync(
            int id,
            int currentUserId,
            bool isAdminOrStaff
        )
        {
            try
            {
                if (!isAdminOrStaff)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xóa bài viết");
                }

                return await DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting news article: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> HardDeleteAsync(object id)
        {
            try
            {
                var article = await _unitOfWork.NewsArticles.GetByIdAsync(id);
                if (article == null)
                    return false;

                // Xóa tất cả NewsArticleTag liên quan trước
                var relatedTags = await _unitOfWork.NewsArticleTags.GetByNewsArticleIdAsync((int)id);
                foreach (var tag in relatedTags)
                {
                    _unitOfWork.NewsArticleTags.Delete(tag);
                }

                // Xóa bài viết
                _unitOfWork.NewsArticles.Delete(article);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Hard deleted article with ID: {NewsArticleId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting news article: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> HardDeleteNewsArticleAsync(
            int id,
            int currentUserId,
            bool isAdmin
        )
        {
            try
            {
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("Chỉ Admin mới có quyền xóa cứng bài viết");
                }

                return await HardDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting news article: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticle>> GetAllAsync()
        {
            try
            {
                return await _unitOfWork.NewsArticles.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all news articles: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticleDto>> GetAllNewsArticlesAsync()
        {
            var articles = await GetAllAsync();
            return articles.Select(MapToDto);
        }

        public async Task<NewsArticle?> GetByIdAsync(object id)
        {
            try
            {
                return await _unitOfWork.NewsArticles.GetByIdAsync(id);
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

        public async Task<NewsArticleDto?> GetNewsArticleByIdAsync(int id)
        {
            var article = await GetByIdAsync(id);
            return article != null ? MapToDto(article) : null;
        }

        public async Task<IEnumerable<NewsArticleDto>> GetNewsByStatusAsync(NewsStatus status)
        {
            try
            {
                var articles = await _unitOfWork.NewsArticles.GetNewsByStatusAsync(status);
                return articles.Select(MapToDto);
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

        public async Task<IEnumerable<NewsArticleDto>> GetNewsByCategoryAsync(int categoryId)
        {
            try
            {
                var articles = await _unitOfWork.NewsArticles.GetNewsByCategoryAsync(categoryId);
                return articles.Select(MapToDto);
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

                var existingArticle = await _unitOfWork.NewsArticles.GetByIdAsync(
                    entity.NewsArticleId
                );
                if (existingArticle == null)
                    throw new InvalidOperationException(
                        $"News article with ID {entity.NewsArticleId} not found"
                    );

                ValidateNewsArticle(entity);

                entity.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.NewsArticles.Update(entity);
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

        public async Task<NewsArticleDto> UpdateNewsArticleAsync(
            int id,
            UpdateNewsArticleDto updateDto,
            int currentUserId,
            bool isAdmin
        )
        {
            var existingArticle = await GetByIdAsync(id);
            if (existingArticle == null)
            {
                throw new InvalidOperationException("Không tìm thấy bài viết");
            }

            if (!isAdmin && existingArticle.CreatedById != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền sửa bài viết này");
            }

            if (updateDto.CategoryId.HasValue)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(
                    updateDto.CategoryId.Value
                );
                if (category == null)
                {
                    throw new ArgumentException("Danh mục không tồn tại");
                }
                existingArticle.CategoryId = updateDto.CategoryId.Value;
            }

            if (!string.IsNullOrEmpty(updateDto.NewsTitle))
                existingArticle.NewsTitle = updateDto.NewsTitle;

            if (updateDto.Headline != null)
                existingArticle.Headline = updateDto.Headline;

            if (!string.IsNullOrEmpty(updateDto.NewsContent))
                existingArticle.NewsContent = updateDto.NewsContent;

            if (updateDto.NewsSource != null)
                existingArticle.NewsSource = updateDto.NewsSource;

            if (updateDto.ImageUrl != null)
                existingArticle.ImageUrl = updateDto.ImageUrl;

            existingArticle.UpdatedById = currentUserId;

            var updatedArticle = await UpdateAsync(existingArticle);
            return MapToDto(updatedArticle);
        }

        public async Task<NewsArticleDto> ChangeNewsStatusAsync(
            int id,
            ChangeNewsStatusDto statusDto,
            int currentUserId
        )
        {
            var article = await GetByIdAsync(id);
            if (article == null)
            {
                throw new InvalidOperationException("Không tìm thấy bài viết");
            }

            article.NewsStatus = statusDto.Status;
            article.UpdatedById = currentUserId;

            var updatedArticle = await UpdateAsync(article);
            return MapToDto(updatedArticle);
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

        private static NewsArticleDto MapToDto(NewsArticle article)
        {
            return new NewsArticleDto
            {
                NewsArticleId = article.NewsArticleId,
                NewsTitle = article.NewsTitle,
                Headline = article.Headline,
                NewsContent = article.NewsContent,
                NewsSource = article.NewsSource,
                ImageUrl = article.ImageUrl,
                CategoryId = article.CategoryId,
                NewsStatus = article.NewsStatus,
                CreatedDate = article.CreatedDate,
                ModifiedDate = article.ModifiedDate,
                CreatedById = article.CreatedById,
                UpdatedById = article.UpdatedById,
            };
        }
    }
}
