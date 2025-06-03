using BusinessObject;
using BusinessObject.Enums;
using Services.DTOs;

namespace Services.Interface
{
    public interface INewsArticleService : IBaseService<NewsArticle>
    {
        Task<NewsArticleDto> CreateNewsArticleAsync(
            CreateNewsArticleDto createDto,
            int currentUserId
        );
        Task<NewsArticleDto> UpdateNewsArticleAsync(
            int id,
            UpdateNewsArticleDto updateDto,
            int currentUserId,
            bool isAdmin
        );
        Task<NewsArticleDto> ChangeNewsStatusAsync(
            int id,
            ChangeNewsStatusDto statusDto,
            int currentUserId
        );
        Task<IEnumerable<NewsArticleDto>> GetNewsByStatusAsync(NewsStatus status);
        Task<IEnumerable<NewsArticleDto>> GetNewsByCategoryAsync(int categoryId);
        Task<NewsArticleDto?> GetNewsArticleByIdAsync(int id);
        Task<IEnumerable<NewsArticleDto>> GetAllNewsArticlesAsync();
        Task<bool> DeleteNewsArticleAsync(int id, int currentUserId, bool isAdminOrStaff);
    }
}
