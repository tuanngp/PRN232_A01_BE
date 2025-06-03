using BusinessObject;
using Services.DTOs;

namespace Services.Interface
{
    public interface ITagService : IBaseService<Tag>
    {
        Task<IEnumerable<TagDto>> GetAllTagsAsync();
        Task<TagDto?> GetTagByIdAsync(int id);
        Task<IEnumerable<TagDto>> SearchTagsAsync(string keyword);
        Task<TagDto> CreateTagAsync(CreateTagDto createDto);
        Task<TagDto> UpdateTagAsync(int id, UpdateTagDto updateDto);
        Task<IEnumerable<TagDto>> GetPopularTagsAsync(int limit);
        Task<IEnumerable<PopularTagDto>> GetTagStatisticsAsync();
        Task<BulkCreateResultDto> CreateBulkTagsAsync(CreateBulkTagsDto bulkDto);
        Task<bool> IsTagNameInUseAsync(string tagName, int? excludeTagId = null);
    }
}
