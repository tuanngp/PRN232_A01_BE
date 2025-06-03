using BusinessObject;
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
                var tags = await _tagService.GetAllAsync();
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
                var tag = await _tagService.GetByIdAsync(id);
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

                var allTags = await _tagService.GetAllAsync();
                var searchResults = allTags
                    .Where(t => t.TagName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return Success(searchResults, $"Tìm kiếm tag với từ khóa '{keyword}' thành công");
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

                var allTags = await _tagService.GetAllAsync();
                var existingTag = allTags.FirstOrDefault(t =>
                    t.TagName.Equals(createDto.TagName, StringComparison.OrdinalIgnoreCase)
                );

                if (existingTag != null)
                {
                    return ValidationError(new { TagName = new[] { "Tên tag đã tồn tại" } });
                }

                var tag = new Tag
                {
                    TagName = createDto.TagName.Trim(),
                    Note = createDto.Note?.Trim(),
                };

                var createdTag = await _tagService.AddAsync(tag);
                return Created(createdTag, "Tạo tag thành công");
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

                var existingTag = await _tagService.GetByIdAsync(id);
                if (existingTag == null)
                {
                    return NotFound("Không tìm thấy tag");
                }

                if (
                    !string.IsNullOrEmpty(updateDto.TagName)
                    && !updateDto.TagName.Equals(
                        existingTag.TagName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    var allTags = await _tagService.GetAllAsync();
                    var duplicateTag = allTags.FirstOrDefault(t =>
                        t.TagId != id
                        && t.TagName.Equals(updateDto.TagName, StringComparison.OrdinalIgnoreCase)
                    );

                    if (duplicateTag != null)
                    {
                        return ValidationError(new { TagName = new[] { "Tên tag đã tồn tại" } });
                    }

                    existingTag.TagName = updateDto.TagName.Trim();
                }

                if (updateDto.Note != null)
                {
                    existingTag.Note = updateDto.Note.Trim();
                }

                var updatedTag = await _tagService.UpdateAsync(existingTag);
                return Success(updatedTag, "Cập nhật tag thành công");
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
                var tag = await _tagService.GetByIdAsync(id);
                if (tag == null)
                {
                    return NotFound("Không tìm thấy tag");
                }

                var result = await _tagService.DeleteAsync(id);
                if (result)
                {
                    return Success("Xóa tag thành công");
                }
                else
                {
                    return Error("Không thể xóa tag");
                }
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
                var allTags = await _tagService.GetAllAsync();

                var statistics = allTags
                    .Select(tag => new TagStatisticsDto
                    {
                        TagId = tag.TagId,
                        TagName = tag.TagName,
                        ArticleCount = tag.NewsArticleTags?.Count ?? 0,
                    })
                    .OrderByDescending(s => s.ArticleCount)
                    .ToList();

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
                if (limit <= 0 || limit > 50)
                {
                    limit = 10;
                }

                var allTags = await _tagService.GetAllAsync();

                var popularTags = allTags
                    .OrderByDescending(tag => tag.NewsArticleTags?.Count ?? 0)
                    .Take(limit)
                    .Select(tag => new PopularTagDto
                    {
                        TagId = tag.TagId,
                        TagName = tag.TagName,
                        UsageCount = tag.NewsArticleTags?.Count ?? 0,
                    })
                    .ToList();

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

                if (bulkDto.TagNames == null || !bulkDto.TagNames.Any())
                {
                    return ValidationError(
                        new { TagNames = new[] { "Danh sách tag không được để trống" } }
                    );
                }

                var allTags = await _tagService.GetAllAsync();
                var existingTagNames = allTags.Select(t => t.TagName.ToLower()).ToHashSet();

                var createdTags = new List<Tag>();
                var duplicates = new List<string>();

                foreach (var tagName in bulkDto.TagNames)
                {
                    var trimmedName = tagName?.Trim();
                    if (string.IsNullOrEmpty(trimmedName))
                        continue;

                    if (existingTagNames.Contains(trimmedName.ToLower()))
                    {
                        duplicates.Add(trimmedName);
                        continue;
                    }

                    var newTag = new Tag { TagName = trimmedName, Note = bulkDto.Note };

                    var createdTag = await _tagService.AddAsync(newTag);
                    createdTags.Add(createdTag);
                    existingTagNames.Add(trimmedName.ToLower());
                }

                var result = new
                {
                    CreatedTags = createdTags,
                    DuplicateTagNames = duplicates,
                    CreatedCount = createdTags.Count,
                    DuplicateCount = duplicates.Count,
                };

                return Success(
                    result,
                    $"Tạo {createdTags.Count} tag thành công. {duplicates.Count} tag bị trùng lặp."
                );
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi tạo tag hàng loạt: {ex.Message}");
            }
        }
    }
}
