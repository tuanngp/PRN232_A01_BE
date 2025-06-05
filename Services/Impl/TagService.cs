using BusinessObject;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.DTOs;
using Services.Interface;

namespace Services.Impl
{
    public class TagService : ITagService
    {
        private readonly ILogger<TagService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public TagService(ILogger<TagService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Tag> AddAsync(Tag entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                ValidateTag(entity);

                if (await IsTagNameInUseAsync(entity.TagName))
                    throw new InvalidOperationException(
                        $"Tag với tên '{entity.TagName}' đã tồn tại"
                    );

                entity.CreatedDate = DateTime.UtcNow;
                await _unitOfWork.Tags.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created new tag with ID: {TagId}", entity.TagId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(object id)
        {
            try
            {
                var tag = await _unitOfWork.Tags.GetByIdAsync(id);
                if (tag == null)
                    return false;

                tag.IsDeleted = true;
                tag.DeletedAt = DateTime.UtcNow;

                _unitOfWork.Tags.Update(tag);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Soft deleted tag with ID: {TagId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            try
            {
                return await _unitOfWork.Tags.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tags: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
        {
            var tags = await GetAllAsync();
            return tags.Select(MapToDto);
        }

        public async Task<Tag?> GetByIdAsync(object id)
        {
            try
            {
                return await _unitOfWork.Tags.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<TagDto?> GetTagByIdAsync(int id)
        {
            var tag = await GetByIdAsync(id);
            return tag != null ? MapToDto(tag) : null;
        }

        public async Task<Tag> UpdateAsync(Tag entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingTag = await _unitOfWork.Tags.GetByIdAsync(entity.TagId);
                if (existingTag == null)
                    throw new InvalidOperationException($"Tag với ID {entity.TagId} không tồn tại");

                ValidateTag(entity);

                if (
                    !string.Equals(
                        existingTag.TagName,
                        entity.TagName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    if (await IsTagNameInUseAsync(entity.TagName, entity.TagId))
                        throw new InvalidOperationException(
                            $"Tag với tên '{entity.TagName}' đã tồn tại"
                        );
                }

                entity.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Tags.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated tag with ID: {TagId}", entity.TagId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<TagDto>> SearchTagsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                throw new ArgumentException("Từ khóa tìm kiếm không được để trống");

            var allTags = await GetAllAsync();
            return allTags
                .Where(t => t.TagName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .Select(MapToDto);
        }

        public async Task<TagDto> CreateTagAsync(CreateTagDto createDto)
        {
            var tag = new Tag { TagName = createDto.TagName.Trim(), Note = createDto.Note?.Trim() };

            var createdTag = await AddAsync(tag);
            return MapToDto(createdTag);
        }

        public async Task<TagDto> UpdateTagAsync(int id, UpdateTagDto updateDto)
        {
            var existingTag = await GetByIdAsync(id);
            if (existingTag == null)
                throw new InvalidOperationException("Không tìm thấy tag");

            if (!string.IsNullOrEmpty(updateDto.TagName))
                existingTag.TagName = updateDto.TagName.Trim();

            if (updateDto.Note != null)
                existingTag.Note = updateDto.Note.Trim();

            var updatedTag = await UpdateAsync(existingTag);
            return MapToDto(updatedTag);
        }

        public async Task<IEnumerable<TagDto>> GetPopularTagsAsync(int limit)
        {
            if (limit <= 0 || limit > 50)
                limit = 10;

            var allTags = await GetAllAsync();
            return allTags
                .OrderByDescending(tag => tag.NewsArticleTags?.Count ?? 0)
                .Take(limit)
                .Select(MapToDto);
        }

        public async Task<IEnumerable<PopularTagDto>> GetTagStatisticsAsync()
        {
            var allTags = await GetAllAsync();
            return allTags
                .Select(tag => new PopularTagDto
                {
                    TagId = tag.TagId,
                    TagName = tag.TagName,
                    UsageCount = tag.NewsArticleTags?.Count ?? 0,
                })
                .OrderByDescending(s => s.UsageCount);
        }

        public async Task<BulkCreateResultDto> CreateBulkTagsAsync(CreateBulkTagsDto bulkDto)
        {
            if (bulkDto.TagNames == null || !bulkDto.TagNames.Any())
                throw new ArgumentException("Danh sách tag không được để trống");

            var result = new BulkCreateResultDto();
            var existingTagNames = (await GetAllAsync())
                .Select(t => t.TagName.ToLower())
                .ToHashSet();

            foreach (var tagName in bulkDto.TagNames)
            {
                var trimmedName = tagName?.Trim();
                if (string.IsNullOrEmpty(trimmedName))
                    continue;

                if (existingTagNames.Contains(trimmedName.ToLower()))
                {
                    result.DuplicateTagNames.Add(trimmedName);
                    continue;
                }

                var newTag = new Tag { TagName = trimmedName, Note = bulkDto.Note };
                var createdTag = await AddAsync(newTag);
                result.CreatedTags.Add(MapToDto(createdTag));
                existingTagNames.Add(trimmedName.ToLower());
            }

            result.CreatedCount = result.CreatedTags.Count;
            result.DuplicateCount = result.DuplicateTagNames.Count;

            return result;
        }

        public async Task<bool> IsTagNameInUseAsync(string tagName, int? excludeTagId = null)
        {
            var allTags = await GetAllAsync();
            return allTags.Any(t =>
                t.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase)
                && (!excludeTagId.HasValue || t.TagId != excludeTagId)
            );
        }

        private void ValidateTag(Tag entity)
        {
            if (string.IsNullOrWhiteSpace(entity.TagName))
                throw new ArgumentException("Tên tag là bắt buộc");

            if (entity.TagName.Length > 50)
                throw new ArgumentException("Tên tag không được vượt quá 50 ký tự");

            if (entity.Note?.Length > 200)
                throw new ArgumentException("Ghi chú không được vượt quá 200 ký tự");
        }

        private static TagDto MapToDto(Tag tag)
        {
            return new TagDto
            {
                TagId = tag.TagId,
                TagName = tag.TagName,
                Note = tag.Note,
                ArticleCount = tag.NewsArticleTags?.Count ?? 0,
                CreatedDate = tag.CreatedDate,
                ModifiedDate = tag.ModifiedDate,
            };
        }
    }
}
