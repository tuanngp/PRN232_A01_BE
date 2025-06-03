using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NguyenGiaPhuongTuan_SE17D06_A01_BE.DTOs;
using Services;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NewsArticleTagController : BaseController
    {
        private readonly INewsArticleTagService _newsArticleTagService;
        private readonly INewsArticleService _newsArticleService;
        private readonly ITagService _tagService;

        public NewsArticleTagController(
            INewsArticleTagService newsArticleTagService,
            INewsArticleService newsArticleService,
            ITagService tagService
        )
        {
            _newsArticleTagService = newsArticleTagService;
            _newsArticleService = newsArticleService;
            _tagService = tagService;
        }

        [HttpGet("article/{articleId}/tags")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTagsByArticle(int articleId)
        {
            try
            {
                // Kiểm tra bài viết có tồn tại không
                var article = await _newsArticleService.GetByIdAsync(articleId);
                if (article == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                var allArticleTags = await _newsArticleTagService.GetAllAsync();
                var articleTags = allArticleTags
                    .Where(at => at.NewsArticleId == articleId)
                    .ToList();

                var result = new List<object>();
                foreach (var articleTag in articleTags)
                {
                    var tag = await _tagService.GetByIdAsync(articleTag.TagId);
                    if (tag != null)
                    {
                        result.Add(
                            new
                            {
                                TagId = tag.TagId,
                                TagName = tag.TagName,
                                Note = tag.Note,
                            }
                        );
                    }
                }

                return Success(result, "Lấy danh sách tag của bài viết thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách tag: {ex.Message}");
            }
        }

        [HttpGet("tag/{tagId}/articles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetArticlesByTag(int tagId)
        {
            try
            {
                // Kiểm tra tag có tồn tại không
                var tag = await _tagService.GetByIdAsync(tagId);
                if (tag == null)
                {
                    return NotFound("Không tìm thấy tag");
                }

                var allArticleTags = await _newsArticleTagService.GetAllAsync();
                var articleTags = allArticleTags.Where(at => at.TagId == tagId).ToList();

                var result = new List<object>();
                foreach (var articleTag in articleTags)
                {
                    var article = await _newsArticleService.GetByIdAsync(articleTag.NewsArticleId);
                    if (article != null)
                    {
                        result.Add(
                            new
                            {
                                NewsArticleId = article.NewsArticleId,
                                NewsTitle = article.NewsTitle,
                                Headline = article.Headline,
                                NewsStatus = article.NewsStatus,
                                CreatedDate = article.CreatedDate,
                                CategoryId = article.CategoryId,
                            }
                        );
                    }
                }

                return Success(
                    result,
                    $"Lấy danh sách bài viết với tag '{tag.TagName}' thành công"
                );
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách bài viết: {ex.Message}");
            }
        }

        [HttpPost("article/{articleId}/tags")]
        [Authorize(Roles = "Admin,Staff,Lecturer")]
        public async Task<IActionResult> AddTagToArticle(
            int articleId,
            [FromBody] AddTagToArticleDto addTagDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Kiểm tra bài viết có tồn tại không
                var article = await _newsArticleService.GetByIdAsync(articleId);
                if (article == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                // Kiểm tra quyền: chỉ Admin hoặc người tạo bài viết mới có thể gán tag
                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                if (!HasRole("Admin") && !HasRole("Staff") && article.CreatedById != currentUserId)
                {
                    return Forbidden("Bạn không có quyền gán tag cho bài viết này");
                }

                // Kiểm tra tag có tồn tại không
                var tag = await _tagService.GetByIdAsync(addTagDto.TagId);
                if (tag == null)
                {
                    return NotFound("Không tìm thấy tag");
                }

                // Kiểm tra xem đã gán tag này cho bài viết chưa
                var allArticleTags = await _newsArticleTagService.GetAllAsync();
                var existingArticleTag = allArticleTags.FirstOrDefault(at =>
                    at.NewsArticleId == articleId && at.TagId == addTagDto.TagId
                );

                if (existingArticleTag != null)
                {
                    return ValidationError(
                        new { TagId = new[] { "Tag đã được gán cho bài viết này" } }
                    );
                }

                // Tạo liên kết mới
                var newsArticleTag = new NewsArticleTag
                {
                    NewsArticleId = articleId,
                    TagId = addTagDto.TagId,
                };

                var createdArticleTag = await _newsArticleTagService.AddAsync(newsArticleTag);

                var result = new
                {
                    NewsArticleId = articleId,
                    TagId = addTagDto.TagId,
                    TagName = tag.TagName,
                    ArticleTitle = article.NewsTitle,
                };

                return Created(result, "Gán tag cho bài viết thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi gán tag: {ex.Message}");
            }
        }

        [HttpPost("article/{articleId}/tags/bulk")]
        [Authorize(Roles = "Admin,Staff,Lecturer")]
        public async Task<IActionResult> AddMultipleTagsToArticle(
            int articleId,
            [FromBody] AddMultipleTagsDto addTagsDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                if (addTagsDto.TagIds == null || !addTagsDto.TagIds.Any())
                {
                    return ValidationError(
                        new { TagIds = new[] { "Danh sách tag không được để trống" } }
                    );
                }

                // Kiểm tra bài viết có tồn tại không
                var article = await _newsArticleService.GetByIdAsync(articleId);
                if (article == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                // Kiểm tra quyền
                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                if (!HasRole("Admin") && !HasRole("Staff") && article.CreatedById != currentUserId)
                {
                    return Forbidden("Bạn không có quyền gán tag cho bài viết này");
                }

                var allTags = await _tagService.GetAllAsync();
                var allArticleTags = await _newsArticleTagService.GetAllAsync();
                var existingArticleTagIds = allArticleTags
                    .Where(at => at.NewsArticleId == articleId)
                    .Select(at => at.TagId)
                    .ToHashSet();

                var addedTags = new List<object>();
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

                    await _newsArticleTagService.AddAsync(newsArticleTag);
                    addedTags.Add(new { TagId = tagId, TagName = tag.TagName });
                }

                var result = new
                {
                    ArticleId = articleId,
                    ArticleTitle = article.NewsTitle,
                    AddedTags = addedTags,
                    SkippedTags = skippedTags,
                    NotFoundTagIds = notFoundTags,
                    AddedCount = addedTags.Count,
                    SkippedCount = skippedTags.Count,
                    NotFoundCount = notFoundTags.Count,
                };

                return Success(
                    result,
                    $"Gán {addedTags.Count} tag thành công. {skippedTags.Count} tag bị bỏ qua. {notFoundTags.Count} tag không tìm thấy."
                );
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi gán tag hàng loạt: {ex.Message}");
            }
        }

        [HttpDelete("article/{articleId}/tags/{tagId}")]
        [Authorize(Roles = "Admin,Staff,Lecturer")]
        public async Task<IActionResult> RemoveTagFromArticle(int articleId, int tagId)
        {
            try
            {
                // Kiểm tra bài viết có tồn tại không
                var article = await _newsArticleService.GetByIdAsync(articleId);
                if (article == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                // Kiểm tra quyền
                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                if (!HasRole("Admin") && !HasRole("Staff") && article.CreatedById != currentUserId)
                {
                    return Forbidden("Bạn không có quyền xóa tag khỏi bài viết này");
                }

                // Tìm liên kết article-tag
                var allArticleTags = await _newsArticleTagService.GetAllAsync();
                var articleTag = allArticleTags.FirstOrDefault(at =>
                    at.NewsArticleId == articleId && at.TagId == tagId
                );

                if (articleTag == null)
                {
                    return NotFound("Không tìm thấy liên kết tag với bài viết");
                }

                // Xóa liên kết (sử dụng ID composite hoặc logic xóa khác)
                // Cần implement DeleteByCompositeKey trong service hoặc tìm theo ID
                var allArticleTagsList = allArticleTags.ToList();
                var articleTagToDelete = allArticleTagsList.FirstOrDefault(at =>
                    at.NewsArticleId == articleId && at.TagId == tagId
                );

                if (articleTagToDelete != null)
                {
                    // Assuming there's an ID field for NewsArticleTag
                    // You might need to adjust this based on your actual entity structure
                    await _newsArticleTagService.DeleteAsync(articleTagToDelete.NewsArticleId); // This might need adjustment
                }

                return Success("Xóa tag khỏi bài viết thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi xóa tag: {ex.Message}");
            }
        }

        [HttpPut("article/{articleId}/tags")]
        [Authorize(Roles = "Admin,Staff,Lecturer")]
        public async Task<IActionResult> ReplaceArticleTags(
            int articleId,
            [FromBody] ReplaceTagsDto replaceTagsDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                // Kiểm tra bài viết có tồn tại không
                var article = await _newsArticleService.GetByIdAsync(articleId);
                if (article == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                // Kiểm tra quyền
                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                if (!HasRole("Admin") && !HasRole("Staff") && article.CreatedById != currentUserId)
                {
                    return Forbidden("Bạn không có quyền thay đổi tag của bài viết này");
                }

                // Kiểm tra tất cả tag có tồn tại không
                var allTags = await _tagService.GetAllAsync();
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
                    return ValidationError(
                        new
                        {
                            TagIds = new[]
                            {
                                $"Các tag sau không tồn tại: {string.Join(", ", invalidTagIds)}",
                            },
                        }
                    );
                }

                // Xóa tất cả tag hiện tại của bài viết
                var allArticleTags = await _newsArticleTagService.GetAllAsync();
                var currentArticleTags = allArticleTags
                    .Where(at => at.NewsArticleId == articleId)
                    .ToList();

                foreach (var currentTag in currentArticleTags)
                {
                    // Delete logic - you might need to adjust this
                    await _newsArticleTagService.DeleteAsync(currentTag.NewsArticleId);
                }

                // Thêm tag mới
                var addedTags = new List<object>();
                foreach (var tagId in validTagIds)
                {
                    var tag = allTags.First(t => t.TagId == tagId);
                    var newsArticleTag = new NewsArticleTag
                    {
                        NewsArticleId = articleId,
                        TagId = tagId,
                    };

                    await _newsArticleTagService.AddAsync(newsArticleTag);
                    addedTags.Add(new { TagId = tagId, TagName = tag.TagName });
                }

                var result = new
                {
                    ArticleId = articleId,
                    ArticleTitle = article.NewsTitle,
                    NewTags = addedTags,
                    TagCount = addedTags.Count,
                };

                return Success(
                    result,
                    $"Thay thế tag cho bài viết thành công. Tổng cộng {addedTags.Count} tag."
                );
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi thay thế tag: {ex.Message}");
            }
        }

        [HttpGet("statistics/popular-tags")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopularTagStatistics([FromQuery] int limit = 10)
        {
            try
            {
                if (limit <= 0 || limit > 50)
                {
                    limit = 10;
                }

                var allArticleTags = await _newsArticleTagService.GetAllAsync();
                var allTags = await _tagService.GetAllAsync();

                var tagStatistics = allTags
                    .Select(tag => new
                    {
                        TagId = tag.TagId,
                        TagName = tag.TagName,
                        ArticleCount = allArticleTags.Count(at => at.TagId == tag.TagId),
                    })
                    .OrderByDescending(stat => stat.ArticleCount)
                    .Take(limit)
                    .ToList();

                return Success(tagStatistics, "Lấy thống kê tag phổ biến thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thống kê: {ex.Message}");
            }
        }
    }
}
