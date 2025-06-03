using BusinessObject;
using Services.DTOs;

namespace Services
{
    public interface ICategoryService : IBaseService<Category>
    {
        Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync();
    }
}
