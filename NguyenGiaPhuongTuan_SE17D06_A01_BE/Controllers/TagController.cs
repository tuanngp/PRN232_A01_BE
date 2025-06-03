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
    public class TagController : BaseController
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> GetTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync();
                return Success(tags, "Lấy danh sách tag thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách tag: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTag(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                if (tag == null)
                {
                    return NotFound("Không tìm thấy tag");
                }

                return Success(tag, "Lấy thông tin tag thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thông tin tag: {ex.Message}");
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchTags([FromQuery] string? keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return ValidationError(
                        new { keyword = new[] { "Từ khóa tìm kiếm không được để trống" } }
                    );
                }

                var searchResults = await _tagService.SearchTagsAsync(keyword);
                return Success(searchResults, $"Tìm kiếm tag với từ khóa '{keyword}' thành công");
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new { keyword = new[] { ex.Message } });
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi tìm kiếm tag: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Lecturer")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var createdTag = await _tagService.CreateTagAsync(createDto);
                return Created(createdTag, "Tạo tag thành công");
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new { TagName = new[] { ex.Message } });
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi tạo tag: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateTag(int id, [FromBody] UpdateTagDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var updatedTag = await _tagService.UpdateTagAsync(id, updateDto);
                return Success(updatedTag, "Cập nhật tag thành công");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật tag: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteTag(int id)
        {
            try
            {
                var result = await _tagService.DeleteAsync(id);
                if (result)
                {
                    return Success("Xóa tag thành công");
                }
                return Error("Không thể xóa tag");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi xóa tag: {ex.Message}");
            }
        }

        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetTagStatistics()
        {
            try
            {
                var statistics = await _tagService.GetTagStatisticsAsync();
                return Success(statistics, "Lấy thống kê tag thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thống kê tag: {ex.Message}");
            }
        }

        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopularTags([FromQuery] int limit = 10)
        {
            try
            {
                var popularTags = await _tagService.GetPopularTagsAsync(limit);
                return Success(popularTags, "Lấy danh sách tag phổ biến thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách tag phổ biến: {ex.Message}");
            }
        }

        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateBulkTags([FromBody] CreateBulkTagsDto bulkDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var result = await _tagService.CreateBulkTagsAsync(bulkDto);
                return Success(
                    result,
                    $"Tạo {result.CreatedCount} tag thành công. {result.DuplicateCount} tag bị trùng lặp."
                );
            }
            catch (ArgumentException ex)
            {
                return ValidationError(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi tạo tag hàng loạt: {ex.Message}");
            }
        }
    }
}
