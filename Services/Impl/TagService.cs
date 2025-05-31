using BusinessObject;
using Repositories.Impl;

namespace Services.Impl
{
    public class TagService : ITagService
    {
        private readonly TagRepository _repository;

        public TagService(TagRepository repository)
        {
            _repository = repository;
        }

        public async Task<Tag> AddAsync(Tag entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Tag> UpdateAsync(Tag entity)
        {
            return await _repository.UpdateAsync(entity);
        }
    }
}
