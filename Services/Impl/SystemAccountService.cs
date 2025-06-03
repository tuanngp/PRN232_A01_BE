using BusinessObject;
using BusinessObject.Enums;
using Microsoft.Extensions.Logging;
using Repositories;
using Repositories.Interface;
using Services.Util;

namespace Services.Impl
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ISystemAccountRepository _accountRepository;
        private readonly ILogger<SystemAccountService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public SystemAccountService(
            ISystemAccountRepository accountRepository,
            ILogger<SystemAccountService> logger,
            IUnitOfWork unitOfWork
        )
        {
            _accountRepository = accountRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemAccount> AddAsync(SystemAccount entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                if (!IsValidEmail(entity.AccountEmail))
                    throw new ArgumentException("Invalid email format");

                if (await _accountRepository.IsEmailExistAsync(entity.AccountEmail))
                    throw new InvalidOperationException("Email already exists");

                entity.AccountPassword = PasswordUtil.HashPassword(entity.AccountPassword);

                entity.IsActive = true;
                entity.CreatedDate = DateTime.UtcNow;

                await _accountRepository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Created new account with ID: {AccountId}",
                    entity.AccountId
                );
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(object id)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(id);
                if (account == null)
                    return false;

                account.IsActive = false;
                account.ModifiedDate = DateTime.UtcNow;

                _accountRepository.Update(account);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Soft deleted account with ID: {AccountId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<SystemAccount>> GetAllAsync()
        {
            try
            {
                return await _accountRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all accounts: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<SystemAccount?> GetByIdAsync(object id)
        {
            try
            {
                return await _accountRepository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account by ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<SystemAccount> GetByEmailAsync(string email)
        {
            try
            {
                if (!IsValidEmail(email))
                    throw new ArgumentException("Invalid email format");

                return await _accountRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting account by email {Email}: {Message}",
                    email,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<IEnumerable<SystemAccount>> GetByRoleAsync(AccountRole role)
        {
            try
            {
                return await _accountRepository.GetByRoleAsync(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting accounts by role {Role}: {Message}",
                    role,
                    ex.Message
                );
                throw;
            }
        }

        public async Task<SystemAccount> UpdateAsync(SystemAccount entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingAccount = await _accountRepository.GetByIdAsync(entity.AccountId);
                if (existingAccount == null)
                    throw new InvalidOperationException(
                        $"Account with ID {entity.AccountId} not found"
                    );

                if (existingAccount.AccountEmail != entity.AccountEmail)
                {
                    if (!IsValidEmail(entity.AccountEmail))
                        throw new ArgumentException("Invalid email format");

                    if (await _accountRepository.IsEmailExistAsync(entity.AccountEmail))
                        throw new InvalidOperationException("Email already exists");
                }

                if (existingAccount.AccountPassword != entity.AccountPassword)
                {
                    entity.AccountPassword = PasswordUtil.HashPassword(entity.AccountPassword);
                }

                entity.ModifiedDate = DateTime.UtcNow;

                _accountRepository.Update(entity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Updated account with ID: {AccountId}", entity.AccountId);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> IsEmailExistAsync(string email)
        {
            try
            {
                if (!IsValidEmail(email))
                    throw new ArgumentException("Invalid email format");

                return await _accountRepository.IsEmailExistAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Message}", ex.Message);
                throw;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
