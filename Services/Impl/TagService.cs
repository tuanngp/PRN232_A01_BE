using BusinessObject;
using BusinessObject.Common;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Interface;

namespace Services.Impl
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly ILogger<TagService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public TagService(
            ITagRepository tagRepository,
            ILogger<TagService> logger,
            IUnitOfWork unitOfWork)
        {
            _tagRepository = tagRepository;
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

                if (await _tagRepository.IsNameExistAsync(entity.TagName))
                    throw new InvalidOperationException("Tag name already exists");

                entity.CreatedDate = DateTime.UtcNow;

                await _tagRepository.AddAsync(entity);
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

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var tag = await _tagRepository.GetByIdAsync(id);
                if (tag == null)
                    return false;

                if (tag.NewsArticleTags.Any(nat => !nat.NewsArticle.IsDeleted))
                    throw new InvalidOperationException("Cannot delete tag that is being used by active news articles");

                tag.IsDeleted = true;
                tag.DeletedAt = DateTime.UtcNow;

                _tagRepository.Update(tag);
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
                return await _tagRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all tags: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            try
            {
                return await _tagRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Tag>> GetByNameAsync(string name)
        {
            try
            {
                return await _tagRepository.GetByNameAsync(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags by name {Name}: {Message}", name, ex.Message);
                throw;
            }
        }

        public async Task<bool> IsNameExistAsync(string name)
        {
            try
            {
                return await _tagRepository.IsNameExistAsync(name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tag name existence: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Tag> UpdateAsync(Tag entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingTag = await _tagRepository.GetByIdAsync(entity.TagId);
                if (existingTag == null)
                    throw new InvalidOperationException($"Tag with ID {entity.TagId} not found");

                ValidateTag(entity);

                if (existingTag.TagName != entity.TagName)
                {
                    if (await _tagRepository.IsNameExistAsync(entity.TagName))
                        throw new InvalidOperationException("Tag name already exists");
                }

                entity.ModifiedDate = DateTime.UtcNow;

                _tagRepository.Update(entity);
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

        private void ValidateTag(Tag entity)
        {
            if (string.IsNullOrEmpty(entity.TagName))
                throw new ArgumentException("Tag name is required");

            if (entity.TagName.Length > 50)
                throw new ArgumentException("Tag name cannot exceed 50 characters");

            if (entity.Note?.Length > 200)
                throw new ArgumentException("Note cannot exceed 200 characters");
        }
    }
}
