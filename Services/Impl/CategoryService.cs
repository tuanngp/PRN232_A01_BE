using BusinessObject;
using BusinessObject.Common;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Interface;

namespace Services.Impl
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            ICategoryRepository categoryRepository,
            ILogger<CategoryService> logger,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<Category> AddAsync(Category entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                ValidateCategory(entity);

                if (entity.ParentCategoryId.HasValue)
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync(entity.ParentCategoryId.Value);
                    if (parentCategory == null)
                        throw new InvalidOperationException($"Parent category with ID {entity.ParentCategoryId.Value} not found");
                }

                entity.CreatedDate = DateTime.UtcNow;
                entity.IsActive = true;

                await _categoryRepository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created new category with ID: {CategoryId}", entity.CategoryId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return false;

                if (category.SubCategories.Any(c => !c.IsDeleted))
                    throw new InvalidOperationException("Cannot delete category with active subcategories");

                if (category.NewsArticles.Any(n => !n.IsDeleted))
                    throw new InvalidOperationException("Cannot delete category with active news articles");

                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                category.IsActive = false;

                _categoryRepository.Update(category);
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

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            try
            {
                return await _categoryRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            try
            {
                return await _categoryRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<Category> UpdateAsync(Category entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingCategory = await _categoryRepository.GetByIdAsync(entity.CategoryId);
                if (existingCategory == null)
                    throw new InvalidOperationException($"Category with ID {entity.CategoryId} not found");

                ValidateCategory(entity);

                if (entity.ParentCategoryId.HasValue && entity.ParentCategoryId != existingCategory.ParentCategoryId)
                {
                    var parentCategory = await _categoryRepository.GetByIdAsync(entity.ParentCategoryId.Value);
                    if (parentCategory == null)
                        throw new InvalidOperationException($"Parent category with ID {entity.ParentCategoryId.Value} not found");

                    if (entity.CategoryId == entity.ParentCategoryId)
                        throw new InvalidOperationException("Category cannot be its own parent");
                }

                entity.ModifiedDate = DateTime.UtcNow;

                _categoryRepository.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated category with ID: {CategoryId}", entity.CategoryId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {Message}", ex.Message);
                throw;
            }
        }

        private void ValidateCategory(Category entity)
        {
            if (string.IsNullOrEmpty(entity.CategoryName))
                throw new ArgumentException("Category name is required");

            if (entity.CategoryName.Length > 100)
                throw new ArgumentException("Category name cannot exceed 100 characters");

            if (entity.CategoryDescription?.Length > 500)
                throw new ArgumentException("Category description cannot exceed 500 characters");
        }
    }
}
