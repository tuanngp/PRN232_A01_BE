using BusinessObject;
using Services.DTOs;

namespace Services.Interface
{
    public interface ICategoryService : IBaseService<Category>
    {
        Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync();
        Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentId);
        Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync();
        Task<CategoryDto> AddAsync(CreateCategoryDto createDto);
        Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto updateDto);
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    }
}
