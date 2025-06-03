using BusinessObject;
using Services.DTOs;

namespace Services.Interface
{
    public interface INewsArticleTagService : IBaseService<NewsArticleTag>
    {
        Task<IEnumerable<NewsArticleTagDto>> GetTagsByArticleAsync(int articleId);
        Task<IEnumerable<ArticleTagSummaryDto>> GetArticlesByTagAsync(int tagId);
        Task<NewsArticleTagDto> AddTagToArticleAsync(
            int articleId,
            AddTagToArticleDto addTagDto,
            int currentUserId,
            bool isAdminOrStaff
        );
        Task<object> AddMultipleTagsToArticleAsync(
            int articleId,
            AddMultipleTagsDto addTagsDto,
            int currentUserId,
            bool isAdminOrStaff
        );
        Task<bool> RemoveTagFromArticleAsync(
            int articleId,
            int tagId,
            int currentUserId,
            bool isAdminOrStaff
        );
        Task<object> ReplaceArticleTagsAsync(
            int articleId,
            ReplaceTagsDto replaceTagsDto,
            int currentUserId,
            bool isAdminOrStaff
        );
        Task<IEnumerable<TagStatisticDto>> GetPopularTagsAsync(int limit);
        Task<bool> ValidateArticleAccessAsync(
            int articleId,
            int currentUserId,
            bool isAdminOrStaff
        );
        Task<bool> ArticleTagExistsAsync(int articleId, int tagId);
        Task<bool> DeleteArticleTagsAsync(IEnumerable<NewsArticleTag> articleTags);
    }
}
