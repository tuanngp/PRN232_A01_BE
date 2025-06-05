using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services.DTOs;
using Services.Interface;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NewsArticleTagController : BaseController
    {
        private readonly INewsArticleTagService _newsArticleTagService;

        public NewsArticleTagController(INewsArticleTagService newsArticleTagService)
        {
            _newsArticleTagService = newsArticleTagService;
        }

        [HttpGet]
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewsArticleTags()
        {
            var newsTag = await _newsArticleTagService.GetAllAsync();
            return Ok(newsTag);
        }

        [HttpGet("article/{articleId}/tags")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTagsByArticle(int articleId)
        {
            try
            {
                var tags = await _newsArticleTagService.GetTagsByArticleAsync(articleId);
                return Success(tags, "Lấy danh sách tag của bài viết thành công");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
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
                var articles = await _newsArticleTagService.GetArticlesByTagAsync(tagId);
                return Success(articles, "Lấy danh sách bài viết thành công");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách bài viết: {ex.Message}");
            }
        }

        [HttpPost("article/{articleId}/tags")]
        [Authorize(Roles = "Staff")]
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

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var isAdminOrStaff = HasRole("Admin") || HasRole("Staff");
                var result = await _newsArticleTagService.AddTagToArticleAsync(
                    articleId,
                    addTagDto,
                    currentUserId.Value,
                    isAdminOrStaff
                );

                return Created(result, "Gán tag cho bài viết thành công");
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi gán tag: {ex.Message}");
            }
        }

        [HttpPost("article/{articleId}/tags/bulk")]
        [Authorize(Roles = "Staff")]
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

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var isAdminOrStaff = HasRole("Admin") || HasRole("Staff");
                var result = await _newsArticleTagService.AddMultipleTagsToArticleAsync(
                    articleId,
                    addTagsDto,
                    currentUserId.Value,
                    isAdminOrStaff
                );

                return Success(result, "Gán tag hàng loạt thành công");
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi gán tag hàng loạt: {ex.Message}");
            }
        }

        [HttpDelete("article/{articleId}/tags/{tagId}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> RemoveTagFromArticle(int articleId, int tagId)
        {
            try
            {
                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var isAdminOrStaff = HasRole("Admin") || HasRole("Staff");
                var result = await _newsArticleTagService.RemoveTagFromArticleAsync(
                    articleId,
                    tagId,
                    currentUserId.Value,
                    isAdminOrStaff
                );

                if (result)
                {
                    return Success("Xóa tag khỏi bài viết thành công");
                }
                return Error("Không thể xóa tag khỏi bài viết");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi xóa tag: {ex.Message}");
            }
        }

        [HttpPut("article/{articleId}/tags")]
        [Authorize(Roles = "Staff")]
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

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var isAdminOrStaff = HasRole("Admin") || HasRole("Staff");
                var result = await _newsArticleTagService.ReplaceArticleTagsAsync(
                    articleId,
                    replaceTagsDto,
                    currentUserId.Value,
                    isAdminOrStaff
                );

                return Success(result, "Thay thế tag cho bài viết thành công");
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
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
                var statistics = await _newsArticleTagService.GetPopularTagsAsync(limit);
                return Success(statistics, "Lấy thống kê tag phổ biến thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thống kê tag: {ex.Message}");
            }
        }
    }
}
