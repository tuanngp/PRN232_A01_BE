using BusinessObject;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.DTOs;
using Services.Interface;

namespace Services.Impl
{
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(ILogger<CategoryService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                return await _unitOfWork.Categories.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                return categories.Select(c => MapToCategoryDto(c));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Category> AddAsync(Category entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (entity.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _unitOfWork.Categories.GetByIdAsync(
                        entity.ParentCategoryId.Value
                    );
                    if (parentCategory == null)
                        throw new InvalidOperationException(
                            $"Parent category with ID {entity.ParentCategoryId.Value} not found"
                        );
                }

                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                await _unitOfWork.Categories.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<CategoryDto> AddAsync(CreateCategoryDto createDto)
        {
            try
            {
                if (createDto == null)
                    throw new ArgumentNullException(nameof(createDto));

                var entity = MapToCategory(createDto);

                if (createDto.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _unitOfWork.Categories.GetByIdAsync(
                        createDto.ParentCategoryId.Value
                    );
                    if (parentCategory == null)
                        throw new InvalidOperationException(
                            $"Parent category with ID {createDto.ParentCategoryId.Value} not found"
                        );
                }

                entity.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.Categories.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var createdCategory = await _unitOfWork.Categories.GetByIdAsync(entity.CategoryId);

                _logger.LogInformation(
                    "Created new category with ID: {CategoryId}",
                    entity.CategoryId
                );
                return MapToCategoryDto(createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Category?> GetByIdAsync(object id)
        {
            try
            {
                return await _unitOfWork.Categories.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting category by ID {Id}: {Message}",
                    id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await GetByIdAsync(id);
                return category != null ? MapToCategoryDto(category) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting category by ID {Id}: {Message}",
                    id,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto updateDto)
        {
            try
            {
                if (updateDto == null)
                    throw new ArgumentNullException(nameof(updateDto));

                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                if (existingCategory == null)
                    throw new InvalidOperationException($"Category with ID {id} not found");

                if (
                    updateDto.ParentCategoryId.HasValue
                    && updateDto.ParentCategoryId != existingCategory.ParentCategoryId
                )
                {
                    var parentCategory = await _unitOfWork.Categories.GetByIdAsync(
                        updateDto.ParentCategoryId.Value
                    );
                    if (parentCategory == null)
                        throw new InvalidOperationException(
                            $"Parent category with ID {updateDto.ParentCategoryId.Value} not found"
                        );

                    if (id == updateDto.ParentCategoryId)
                        throw new InvalidOperationException("Category cannot be its own parent");
                }

                UpdateCategoryFromDto(existingCategory, updateDto);
                existingCategory.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Categories.Update(existingCategory);
                await _unitOfWork.SaveChangesAsync();

                var updatedCategory = await _unitOfWork.Categories.GetByIdAsync(id);
                _logger.LogInformation("Updated category with ID: {CategoryId}", id);
                return MapToCategoryDto(updatedCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Category> UpdateAsync(Category entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingCategory = await _unitOfWork.Categories.GetByIdAsync(entity.CategoryId);
                if (existingCategory == null)
                    throw new InvalidOperationException(
                        $"Category with ID {entity.CategoryId} not found"
                    );

                if (entity.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _unitOfWork.Categories.GetByIdAsync(
                        entity.ParentCategoryId.Value
                    );
                    if (parentCategory == null)
                        throw new InvalidOperationException(
                            $"Parent category with ID {entity.ParentCategoryId.Value} not found"
                        );

                    if (entity.CategoryId == entity.ParentCategoryId)
                        throw new InvalidOperationException("Category cannot be its own parent");
                }

                entity.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.Categories.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryTreeDto>> GetCategoryTreeAsync()
        {
            try
            {
                var allCategories = await _unitOfWork.Categories.GetAllAsync();
                var rootCategories = allCategories.Where(c =>
                    !c.ParentCategoryId.HasValue && !c.IsDeleted
                );

                return BuildCategoryTree(rootCategories, allCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category tree: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(int parentId)
        {
            try
            {
                var allCategories = await _unitOfWork.Categories.GetAllAsync();
                var subCategories = allCategories
                    .Where(c => c.ParentCategoryId == parentId && !c.IsDeleted)
                    .ToList();

                return subCategories.Select(c => MapToCategoryDto(c));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting subcategories for parent ID {ParentId}: {Message}",
                    parentId,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<IEnumerable<CategoryDto>> GetRootCategoriesAsync()
        {
            try
            {
                var allCategories = await _unitOfWork.Categories.GetAllAsync();
                var rootCategories = allCategories
                    .Where(c => !c.ParentCategoryId.HasValue && !c.IsDeleted)
                    .ToList();

                return rootCategories.Select(c => MapToCategoryDto(c));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting root categories: {Message}", ex.Message);
                throw;
            }
        }

        private IEnumerable<CategoryTreeDto> BuildCategoryTree(
            IEnumerable<Category> categories,
            IEnumerable<Category> allCategories
        )
        {
            return categories.Select(c => new CategoryTreeDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription,
                IsActive = c.IsActive,
                Children = BuildCategoryTree(
                        allCategories.Where(child =>
                            child.ParentCategoryId == c.CategoryId && !child.IsDeleted
                        ),
                        allCategories
                    )
                    .ToList(),
            });
        }

        public async Task<bool> DeleteAsync(object id)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                    return false;

                if (category.SubCategories.Any(c => !c.IsDeleted))
                    throw new InvalidOperationException(
                        "Cannot delete category with active subcategories"
                    );

                if (category.NewsArticles.Any(n => !n.IsDeleted))
                    throw new InvalidOperationException(
                        "Cannot delete category with active news articles"
                    );

                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                category.IsActive = false;

                _unitOfWork.Categories.Update(category);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Soft deleted category with ID: {CategoryId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {Message}", ex.Message);
                throw;
            }
        }

        private CategoryDto MapToCategoryDto(Category category)
        {
            if (category == null)
                return null;

            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                IsActive = category.IsActive,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategory =
                    category.ParentCategory != null
                        ? MapToCategoryBasicInfoDto(category.ParentCategory)
                        : null,
                SubCategories = category
                    .SubCategories.Where(sc => !sc.IsDeleted)
                    .Select(sc => MapToCategoryBasicInfoDto(sc))
                    .ToList(),
            };
        }

        private CategoryBasicInfoDto MapToCategoryBasicInfoDto(Category category)
        {
            if (category == null)
                return null;

            return new CategoryBasicInfoDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription,
                IsActive = category.IsActive,
            };
        }

        private Category MapToCategory(CreateCategoryDto dto)
        {
            return new Category
            {
                CategoryName = dto.CategoryName,
                CategoryDescription = dto.CategoryDescription,
                ParentCategoryId = dto.ParentCategoryId,
                IsActive = true,
            };
        }

        private void UpdateCategoryFromDto(Category category, UpdateCategoryDto dto)
        {
            if (dto.CategoryName != null)
                category.CategoryName = dto.CategoryName;

            if (dto.CategoryDescription != null)
                category.CategoryDescription = dto.CategoryDescription;

            if (dto.ParentCategoryId.HasValue)
                category.ParentCategoryId = dto.ParentCategoryId;

            if (dto.IsActive.HasValue)
                category.IsActive = dto.IsActive.Value;
        }
    }
}
