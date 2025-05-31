using BusinessObject;
using Repositories.Impl;

namespace Services.Impl
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly SystemAccountRepository _repository;

        public SystemAccountService(SystemAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<SystemAccount> AddAsync(SystemAccount entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<SystemAccount>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<SystemAccount?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<SystemAccount> UpdateAsync(SystemAccount entity)
        {
            return await _repository.UpdateAsync(entity);
        }
    }
}
