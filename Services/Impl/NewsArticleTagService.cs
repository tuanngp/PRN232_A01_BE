using BusinessObject;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.DTOs;
using Services.Interface;

namespace Services.Impl
{
    public class NewsArticleTagService : INewsArticleTagService
    {
        private readonly ILogger<NewsArticleTagService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public NewsArticleTagService(ILogger<NewsArticleTagService> logger, IUnitOfWork unitOfWork)
        {
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

                await _unitOfWork.NewsArticleTags.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Created new news article tag mapping: Article ID {ArticleId}, Tag ID {TagId}",
                    entity.NewsArticleId,
                    entity.TagId
                );
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating news article tag mapping: {Message}",
                    ex.Message
                );
                throw;
            }
        }

        public async Task<bool> DeleteAsync(object id)
        {
            try
            {
                var mapping = await _unitOfWork.NewsArticleTags.GetByIdAsync(id);
                if (mapping == null)
                    return false;

                _unitOfWork.NewsArticleTags.Delete(mapping);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Deleted news article tag mapping: Article ID {ArticleId}, Tag ID {TagId}",
                    mapping.NewsArticleId,
                    mapping.TagId
                );
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting news article tag mapping: {Message}",
                    ex.Message
                );
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticleTag>> GetAllAsync()
        {
            try
            {
                return await _unitOfWork.NewsArticleTags.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting all news article tag mappings: {Message}",
                    ex.Message
                );
                throw;
            }
        }

        public async Task<NewsArticleTag?> GetByIdAsync(object id)
        {
            try
            {
                return await _unitOfWork.NewsArticleTags.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting news article tag mapping by ID {Id}: {Message}",
                    id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<NewsArticleTag> UpdateAsync(NewsArticleTag entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingMapping = await _unitOfWork.NewsArticleTags.GetByIdAsync(
                    entity.NewsArticleId
                );
                if (existingMapping == null)
                    throw new InvalidOperationException($"News article tag mapping not found");

                await ValidateNewsArticleTag(entity);

                _unitOfWork.NewsArticleTags.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Updated news article tag mapping: Article ID {ArticleId}, Tag ID {TagId}",
                    entity.NewsArticleId,
                    entity.TagId
                );
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating news article tag mapping: {Message}",
                    ex.Message
                );
                throw;
            }
        }

        public async Task<IEnumerable<NewsArticleTagDto>> GetTagsByArticleAsync(int articleId)
        {
            var article = await _unitOfWork.NewsArticles.GetByIdAsync(articleId);
            if (article == null)
            {
                throw new InvalidOperationException("Không tìm thấy bài viết");
            }

            var articleTags = await _unitOfWork.NewsArticleTags.GetByNewsArticleIdAsync(articleId);
            var result = new List<NewsArticleTagDto>();

            foreach (var articleTag in articleTags)
            {
                var tag = await _unitOfWork.Tags.GetByIdAsync(articleTag.TagId);
                if (tag != null)
                {
                    result.Add(
                        new NewsArticleTagDto
                        {
                            NewsArticleId = articleId,
                            TagId = tag.TagId,
                            TagName = tag.TagName,
                        }
                    );
                }
            }

            return result;
        }

        public async Task<IEnumerable<ArticleTagSummaryDto>> GetArticlesByTagAsync(int tagId)
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(tagId);
            if (tag == null)
            {
                throw new InvalidOperationException("Không tìm thấy tag");
            }

            var articleTags = await _unitOfWork.NewsArticleTags.GetByTagIdAsync(tagId);
            var result = new List<ArticleTagSummaryDto>();

            foreach (var articleTag in articleTags)
            {
                var article = await _unitOfWork.NewsArticles.GetByIdAsync(articleTag.NewsArticleId);
                if (article != null)
                {
                    result.Add(
                        new ArticleTagSummaryDto
                        {
                            NewsArticleId = article.NewsArticleId,
                            NewsTitle = article.NewsTitle,
                            Tags = new List<NewsArticleTagDto>
                            {
                                new NewsArticleTagDto
                                {
                                    NewsArticleId = article.NewsArticleId,
                                    TagId = tagId,
                                    TagName = tag.TagName,
                                },
                            },
                        }
                    );
                }
            }

            return result;
        }

        public async Task<NewsArticleTagDto> AddTagToArticleAsync(
            int articleId,
            AddTagToArticleDto addTagDto,
            int currentUserId,
            bool isAdminOrStaff
        )
        {
            await ValidateArticleAccessAsync(articleId, currentUserId, isAdminOrStaff);

            var tag = await _unitOfWork.Tags.GetByIdAsync(addTagDto.TagId);
            if (tag == null)
            {
                throw new InvalidOperationException("Không tìm thấy tag");
            }

            if (await ArticleTagExistsAsync(articleId, addTagDto.TagId))
            {
                throw new InvalidOperationException("Tag đã được gán cho bài viết này");
            }

            var newsArticleTag = new NewsArticleTag
            {
                NewsArticleId = articleId,
                TagId = addTagDto.TagId,
            };

            await AddAsync(newsArticleTag);

            return new NewsArticleTagDto
            {
                NewsArticleId = articleId,
                TagId = tag.TagId,
                TagName = tag.TagName,
            };
        }

        public async Task<object> AddMultipleTagsToArticleAsync(
            int articleId,
            AddMultipleTagsDto addTagsDto,
            int currentUserId,
            bool isAdminOrStaff
        )
        {
            await ValidateArticleAccessAsync(articleId, currentUserId, isAdminOrStaff);

            var allTags = await _unitOfWork.Tags.GetAllAsync();
            var existingArticleTagIds = (await GetTagsByArticleAsync(articleId))
                .Select(t => t.TagId)
                .ToHashSet();

            var addedTags = new List<NewsArticleTagDto>();
            var skippedTags = new List<object>();
            var notFoundTags = new List<int>();

            foreach (var tagId in addTagsDto.TagIds.Distinct())
            {
                var tag = allTags.FirstOrDefault(t => t.TagId == tagId);
                if (tag == null)
                {
                    notFoundTags.Add(tagId);
                    continue;
                }

                if (existingArticleTagIds.Contains(tagId))
                {
                    skippedTags.Add(
                        new
                        {
                            TagId = tagId,
                            TagName = tag.TagName,
                            Reason = "Đã tồn tại",
                        }
                    );
                    continue;
                }

                var newsArticleTag = new NewsArticleTag
                {
                    NewsArticleId = articleId,
                    TagId = tagId,
                };

                await AddAsync(newsArticleTag);
                addedTags.Add(
                    new NewsArticleTagDto
                    {
                        NewsArticleId = articleId,
                        TagId = tagId,
                        TagName = tag.TagName,
                    }
                );
            }

            var article = await _unitOfWork.NewsArticles.GetByIdAsync(articleId);
            return new
            {
                ArticleId = articleId,
                ArticleTitle = article?.NewsTitle,
                AddedTags = addedTags,
                SkippedTags = skippedTags,
                NotFoundTagIds = notFoundTags,
                AddedCount = addedTags.Count,
                SkippedCount = skippedTags.Count,
                NotFoundCount = notFoundTags.Count,
            };
        }

        public async Task<bool> RemoveTagFromArticleAsync(
            int articleId,
            int tagId,
            int currentUserId,
            bool isAdminOrStaff
        )
        {
            await ValidateArticleAccessAsync(articleId, currentUserId, isAdminOrStaff);

            var articleTags = await GetAllAsync();
            var articleTag = articleTags.FirstOrDefault(at =>
                at.NewsArticleId == articleId && at.TagId == tagId
            );

            if (articleTag == null)
            {
                throw new InvalidOperationException("Không tìm thấy liên kết tag với bài viết");
            }

            return await DeleteAsync(articleTag.NewsArticleId);
        }

        public async Task<object> ReplaceArticleTagsAsync(
            int articleId,
            ReplaceTagsDto replaceTagsDto,
            int currentUserId,
            bool isAdminOrStaff
        )
        {
            await ValidateArticleAccessAsync(articleId, currentUserId, isAdminOrStaff);

            var allTags = await _unitOfWork.Tags.GetAllAsync();
            var validTagIds = new List<int>();
            var invalidTagIds = new List<int>();

            if (replaceTagsDto.TagIds != null)
            {
                foreach (var tagId in replaceTagsDto.TagIds.Distinct())
                {
                    if (allTags.Any(t => t.TagId == tagId))
                    {
                        validTagIds.Add(tagId);
                    }
                    else
                    {
                        invalidTagIds.Add(tagId);
                    }
                }
            }

            if (invalidTagIds.Any())
            {
                throw new InvalidOperationException(
                    $"Các tag sau không tồn tại: {string.Join(", ", invalidTagIds)}"
                );
            }

            var currentArticleTags = await _unitOfWork.NewsArticleTags.GetByNewsArticleIdAsync(
                articleId
            );
            await DeleteArticleTagsAsync(currentArticleTags);

            var addedTags = new List<NewsArticleTagDto>();
            foreach (var tagId in validTagIds)
            {
                var tag = allTags.First(t => t.TagId == tagId);
                var newsArticleTag = new NewsArticleTag
                {
                    NewsArticleId = articleId,
                    TagId = tagId,
                };

                await AddAsync(newsArticleTag);
                addedTags.Add(
                    new NewsArticleTagDto
                    {
                        NewsArticleId = articleId,
                        TagId = tagId,
                        TagName = tag.TagName,
                    }
                );
            }

            var article = await _unitOfWork.NewsArticles.GetByIdAsync(articleId);
            return new
            {
                ArticleId = articleId,
                ArticleTitle = article?.NewsTitle,
                NewTags = addedTags,
                TagCount = addedTags.Count,
            };
        }

        public async Task<IEnumerable<TagStatisticDto>> GetPopularTagsAsync(int limit)
        {
            if (limit <= 0 || limit > 50)
            {
                limit = 10;
            }

            var allArticleTags = await GetAllAsync();
            var allTags = await _unitOfWork.Tags.GetAllAsync();

            var tagStatistics = allTags
                .Select(tag => new TagStatisticDto
                {
                    TagId = tag.TagId,
                    TagName = tag.TagName,
                    ArticleCount = allArticleTags.Count(at => at.TagId == tag.TagId),
                })
                .OrderByDescending(stat => stat.ArticleCount)
                .Take(limit)
                .ToList();

            return tagStatistics;
        }

        public async Task<bool> ValidateArticleAccessAsync(
            int articleId,
            int currentUserId,
            bool isAdminOrStaff
        )
        {
            var article = await _unitOfWork.NewsArticles.GetByIdAsync(articleId);
            if (article == null)
            {
                throw new InvalidOperationException("Không tìm thấy bài viết");
            }

            if (!isAdminOrStaff && article.CreatedById != currentUserId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền thực hiện thao tác này");
            }

            return true;
        }

        public async Task<bool> ArticleTagExistsAsync(int articleId, int tagId)
        {
            var articleTags = await GetAllAsync();
            return articleTags.Any(at => at.NewsArticleId == articleId && at.TagId == tagId);
        }

        public async Task<bool> DeleteArticleTagsAsync(IEnumerable<NewsArticleTag> articleTags)
        {
            try
            {
                foreach (var articleTag in articleTags)
                {
                    await DeleteAsync(articleTag.NewsArticleId);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article tags: {Message}", ex.Message);
                throw;
            }
        }

        private async Task ValidateNewsArticleTag(NewsArticleTag entity)
        {
            if (entity.NewsArticleId <= 0)
                throw new ArgumentException("Invalid news article ID");

            if (entity.TagId <= 0)
                throw new ArgumentException("Invalid tag ID");

            var newsArticle = await _unitOfWork.NewsArticles.GetByIdAsync(entity.NewsArticleId);
            if (newsArticle == null)
                throw new InvalidOperationException(
                    $"News article with ID {entity.NewsArticleId} not found"
                );

            if (newsArticle.IsDeleted)
                throw new InvalidOperationException("Cannot tag a deleted news article");

            var tag = await _unitOfWork.Tags.GetByIdAsync(entity.TagId);
            if (tag == null)
                throw new InvalidOperationException($"Tag with ID {entity.TagId} not found");

            if (tag.IsDeleted)
                throw new InvalidOperationException("Cannot use a deleted tag");
        }
    }
}
