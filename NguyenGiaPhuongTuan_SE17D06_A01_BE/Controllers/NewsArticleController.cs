using BusinessObject;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using NguyenGiaPhuongTuan_SE17D06_A01_BE.DTOs;
using Services;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NewsArticleController : BaseController
    {
        private readonly INewsArticleService _newsArticleService;
        private readonly ICategoryService _categoryService;

        public NewsArticleController(
            INewsArticleService newsArticleService,
            ICategoryService categoryService
        )
        {
            _newsArticleService = newsArticleService;
            _categoryService = categoryService;
        }

        [HttpGet]
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewsArticles()
        {
            try
            {
                var articles = await _newsArticleService.GetAllAsync();
                return Success(articles, "Lấy danh sách bài viết thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách bài viết: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewsArticle(int id)
        {
            try
            {
                var article = await _newsArticleService.GetByIdAsync(id);
                if (article == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                return Success(article, "Lấy thông tin bài viết thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thông tin bài viết: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Lecturer")]
        public async Task<IActionResult> CreateNewsArticle(
            [FromBody] CreateNewsArticleDto createDto
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

                // Kiểm tra category có tồn tại không
                var category = await _categoryService.GetByIdAsync(createDto.CategoryId);
                if (category == null)
                {
                    return ValidationError(new { CategoryId = new[] { "Danh mục không tồn tại" } });
                }

                var article = new NewsArticle
                {
                    NewsTitle = createDto.NewsTitle,
                    Headline = createDto.Headline,
                    NewsContent = createDto.NewsContent,
                    NewsSource = createDto.NewsSource,
                    CategoryId = createDto.CategoryId,
                    CreatedById = currentUserId.Value,
                    CreatedDate = DateTime.UtcNow,
                    NewsStatus = NewsStatus.Inactive, // Mặc định là Inactive, cần Admin duyệt
                };

                var createdArticle = await _newsArticleService.AddAsync(article);
                return Created(createdArticle, "Tạo bài viết thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi tạo bài viết: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff,Lecturer")]
        public async Task<IActionResult> UpdateNewsArticle(
            int id,
            [FromBody] UpdateNewsArticleDto updateDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var existingArticle = await _newsArticleService.GetByIdAsync(id);
                if (existingArticle == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                // Kiểm tra quyền: chỉ Admin hoặc người tạo bài viết mới có thể sửa
                if (!HasRole("Admin") && existingArticle.CreatedById != currentUserId)
                {
                    return Forbidden("Bạn không có quyền sửa bài viết này");
                }

                // Kiểm tra category nếu có thay đổi
                if (updateDto.CategoryId.HasValue)
                {
                    var category = await _categoryService.GetByIdAsync(updateDto.CategoryId.Value);
                    if (category == null)
                    {
                        return ValidationError(
                            new { CategoryId = new[] { "Danh mục không tồn tại" } }
                        );
                    }
                    existingArticle.CategoryId = updateDto.CategoryId.Value;
                }

                // Cập nhật các trường
                if (!string.IsNullOrEmpty(updateDto.NewsTitle))
                    existingArticle.NewsTitle = updateDto.NewsTitle;

                if (updateDto.Headline != null)
                    existingArticle.Headline = updateDto.Headline;

                if (!string.IsNullOrEmpty(updateDto.NewsContent))
                    existingArticle.NewsContent = updateDto.NewsContent;

                if (updateDto.NewsSource != null)
                    existingArticle.NewsSource = updateDto.NewsSource;

                existingArticle.UpdatedById = currentUserId.Value;

                var updatedArticle = await _newsArticleService.UpdateAsync(existingArticle);
                return Success(updatedArticle, "Cập nhật bài viết thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật bài viết: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteNewsArticle(int id)
        {
            try
            {
                var article = await _newsArticleService.GetByIdAsync(id);
                if (article == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                // Kiểm tra quyền: chỉ Admin hoặc Staff mới có thể xóa
                if (!HasRole("Admin") && !HasRole("Staff"))
                {
                    return Forbidden("Bạn không có quyền xóa bài viết");
                }

                var result = await _newsArticleService.DeleteAsync(id);
                if (result)
                {
                    return Success("Xóa bài viết thành công");
                }
                else
                {
                    return Error("Không thể xóa bài viết");
                }
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi xóa bài viết: {ex.Message}");
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ChangeNewsStatus(
            int id,
            [FromBody] ChangeStatusDto statusDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var article = await _newsArticleService.GetByIdAsync(id);
                if (article == null)
                {
                    return NotFound("Không tìm thấy bài viết");
                }

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                article.NewsStatus = statusDto.Status;
                article.UpdatedById = currentUserId.Value;

                var updatedArticle = await _newsArticleService.UpdateAsync(article);
                return Success(updatedArticle, "Cập nhật trạng thái bài viết thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }
    }
}
