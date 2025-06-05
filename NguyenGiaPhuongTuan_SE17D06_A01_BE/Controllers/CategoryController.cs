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
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [EnableQuery]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                //var categories = await _categoryService.GetAllCategoriesAsync();
                //return Success(categories, "Lấy danh sách danh mục thành công");
                var categories = await _categoryService.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách danh mục: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound("Không tìm thấy danh mục");
                }

                return Success(category, "Lấy thông tin danh mục thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thông tin danh mục: {ex.Message}");
            }
        }

        [HttpGet("{parentId}/subcategories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSubCategories(int parentId)
        {
            try
            {
                var subCategories = await _categoryService.GetSubCategoriesAsync(parentId);
                return Success(subCategories, "Lấy danh sách danh mục con thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách danh mục con: {ex.Message}");
            }
        }

        [HttpGet("root")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRootCategories()
        {
            try
            {
                var rootCategories = await _categoryService.GetRootCategoriesAsync();
                return Success(rootCategories, "Lấy danh sách danh mục gốc thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách danh mục gốc: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var createdCategory = await _categoryService.AddAsync(createDto);
                return Created(createdCategory, "Tạo danh mục thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi tạo danh mục: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] UpdateCategoryDto updateDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var updatedCategory = await _categoryService.UpdateAsync(id, updateDto);
                return Success(updatedCategory, "Cập nhật danh mục thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật danh mục: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound("Không tìm thấy danh mục");
                }

                var result = await _categoryService.DeleteAsync(id);
                if (result)
                {
                    return Success("Xóa danh mục thành công");
                }
                else
                {
                    return Error("Không thể xóa danh mục");
                }
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi xóa danh mục: {ex.Message}");
            }
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound("Không tìm thấy danh mục");
                }

                var updateDto = new UpdateCategoryDto { IsActive = !category.IsActive };

                var updatedCategory = await _categoryService.UpdateAsync(
                    category.CategoryId,
                    updateDto
                );

                string message = updatedCategory.IsActive
                    ? "Kích hoạt danh mục thành công"
                    : "Tạm dừng danh mục thành công";
                return Success(updatedCategory, message);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi thay đổi trạng thái danh mục: {ex.Message}");
            }
        }

        [HttpGet("tree")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryTree()
        {
            try
            {
                var categoryTree = await _categoryService.GetCategoryTreeAsync();

                return Success(categoryTree, "Lấy cây danh mục thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy cây danh mục: {ex.Message}");
            }
        }
    }
}
