using BusinessObject.Enums;
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
    public class NewsArticleController : BaseController
    {
        private readonly INewsArticleService _newsArticleService;

        public NewsArticleController(INewsArticleService newsArticleService)
        {
            _newsArticleService = newsArticleService;
        }

        [HttpGet]
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> GetNewsArticles()
        {
            try
            {
                var articles = await _newsArticleService.GetAllNewsArticlesAsync();
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
                var article = await _newsArticleService.GetNewsArticleByIdAsync(id);
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

                var createdArticle = await _newsArticleService.CreateNewsArticleAsync(
                    createDto,
                    currentUserId.Value
                );
                return Created(createdArticle, "Tạo bài viết thành công");
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new { Message = ex.Message });
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

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var updatedArticle = await _newsArticleService.UpdateNewsArticleAsync(
                    id,
                    updateDto,
                    currentUserId.Value,
                    HasRole("Admin")
                );
                return Success(updatedArticle, "Cập nhật bài viết thành công");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new { Message = ex.Message });
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
                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var isAdminOrStaff = HasRole("Admin") || HasRole("Staff");
                var result = await _newsArticleService.DeleteNewsArticleAsync(
                    id,
                    currentUserId.Value,
                    isAdminOrStaff
                );

                if (result)
                {
                    return Success("Xóa bài viết thành công");
                }
                return Error("Không thể xóa bài viết");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbidden(ex.Message);
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
            [FromBody] ChangeNewsStatusDto statusDto
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

                var updatedArticle = await _newsArticleService.ChangeNewsStatusAsync(
                    id,
                    statusDto,
                    currentUserId.Value
                );
                return Success(updatedArticle, "Cập nhật trạng thái bài viết thành công");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }
    }
}
