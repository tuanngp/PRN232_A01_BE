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
                var categories = await _categoryService.GetAllAsync();
                return Success(categories, "Lấy danh sách danh mục thành công");
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
                var category = await _categoryService.GetByIdAsync(id);
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
                var allCategories = await _categoryService.GetAllAsync();
                var subCategories = allCategories
                    .Where(c => c.ParentCategoryId == parentId)
                    .ToList();

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
                var allCategories = await _categoryService.GetAllAsync();
                var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();

                return Success(rootCategories, "Lấy danh sách danh mục gốc thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách danh mục gốc: {ex.Message}");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                if (createDto.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _categoryService.GetByIdAsync(
                        createDto.ParentCategoryId.Value
                    );
                    if (parentCategory == null)
                    {
                        return ValidationError(
                            new { ParentCategoryId = new[] { "Danh mục cha không tồn tại" } }
                        );
                    }
                }

                var category = new Category
                {
                    CategoryName = createDto.CategoryName,
                    CategoryDescription = createDto.CategoryDescription,
                    ParentCategoryId = createDto.ParentCategoryId,
                    IsActive = true,
                };

                var createdCategory = await _categoryService.AddAsync(category);
                return Created(createdCategory, "Tạo danh mục thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi tạo danh mục: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
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

                var existingCategory = await _categoryService.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound("Không tìm thấy danh mục");
                }

                if (updateDto.ParentCategoryId.HasValue)
                {
                    if (updateDto.ParentCategoryId.Value == id)
                    {
                        return ValidationError(
                            new { ParentCategoryId = new[] { "Danh mục không thể tự tham chiếu" } }
                        );
                    }

                    var parentCategory = await _categoryService.GetByIdAsync(
                        updateDto.ParentCategoryId.Value
                    );
                    if (parentCategory == null)
                    {
                        return ValidationError(
                            new { ParentCategoryId = new[] { "Danh mục cha không tồn tại" } }
                        );
                    }

                    existingCategory.ParentCategoryId = updateDto.ParentCategoryId;
                }
                else if (updateDto.ParentCategoryId == null)
                {
                    existingCategory.ParentCategoryId = null;
                }

                if (!string.IsNullOrEmpty(updateDto.CategoryName))
                    existingCategory.CategoryName = updateDto.CategoryName;

                if (updateDto.CategoryDescription != null)
                    existingCategory.CategoryDescription = updateDto.CategoryDescription;

                if (updateDto.IsActive.HasValue)
                    existingCategory.IsActive = updateDto.IsActive.Value;

                var updatedCategory = await _categoryService.UpdateAsync(existingCategory);
                return Success(updatedCategory, "Cập nhật danh mục thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật danh mục: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound("Không tìm thấy danh mục");
                }

                var allCategories = await _categoryService.GetAllAsync();
                var hasSubCategories = allCategories.Any(c => c.ParentCategoryId == id);
                if (hasSubCategories)
                {
                    return Error(
                        "Không thể xóa danh mục có danh mục con. Vui lòng xóa danh mục con trước.",
                        400
                    );
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
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ToggleCategoryStatus(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound("Không tìm thấy danh mục");
                }

                category.IsActive = !category.IsActive;
                var updatedCategory = await _categoryService.UpdateAsync(category);

                string message = category.IsActive
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
