using BusinessObject;
using BusinessObject.Enums;
using Microsoft.Extensions.Logging;
using Repositories;
using Services.DTOs;
using Services.Interface;
using Services.Util;

namespace Services.Impl
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ILogger<SystemAccountService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public SystemAccountService(ILogger<SystemAccountService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<SystemAccount> AddAsync(SystemAccount entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                ValidateEmail(entity.AccountEmail);

                if (await IsEmailInUseAsync(entity.AccountEmail))
                    throw new InvalidOperationException("Email đã được sử dụng");

                entity.AccountPassword = PasswordUtil.HashPassword(entity.AccountPassword);
                entity.IsActive = true;
                entity.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.SystemAccounts.AddAsync(entity);
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
                var account = await _unitOfWork.SystemAccounts.GetByIdAsync(id);
                if (account == null)
                    return false;

                account.IsActive = false;
                account.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.SystemAccounts.Update(account);
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

        public async Task<bool> HardDeleteAsync(object id)
        {
            try
            {
                var account = await _unitOfWork.SystemAccounts.GetByIdAsync(id);
                if (account == null)
                    return false;

                // Check if account has news articles
                if ((account.CreatedNewsArticles != null && account.CreatedNewsArticles.Any()) ||
                    (account.UpdatedNewsArticles != null && account.UpdatedNewsArticles.Any()))
                    throw new InvalidOperationException(
                        "Cannot hard delete account with news articles"
                    );

                _unitOfWork.SystemAccounts.Delete(account);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Hard deleted account with ID: {AccountId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting account: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> HardDeleteAccountAsync(int id, int currentUserId, bool isAdmin)
        {
            try
            {
                if (!isAdmin)
                {
                    throw new UnauthorizedAccessException("Chỉ Admin mới có quyền xóa cứng tài khoản");
                }

                // Prevent self-deletion
                if (id == currentUserId)
                {
                    throw new InvalidOperationException("Không thể xóa tài khoản của chính mình");
                }

                return await HardDeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting account: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<SystemAccount>> GetAllAsync()
        {
            try
            {
                return await _unitOfWork.SystemAccounts.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all accounts: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<SystemAccountDto>> GetAllAccountsAsync()
        {
            var accounts = await GetAllAsync();
            return accounts.Select(MapToDto);
        }

        public async Task<SystemAccount?> GetByIdAsync(object id)
        {
            try
            {
                return await _unitOfWork.SystemAccounts.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account by ID {Id}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<SystemAccountDto?> GetAccountByIdAsync(int id)
        {
            var account = await GetByIdAsync(id);
            return account != null ? MapToDto(account) : null;
        }

        public async Task<SystemAccountDto?> GetAccountByEmailAsync(string email)
        {
            ValidateEmail(email);
            var account = await _unitOfWork.SystemAccounts.GetByEmailAsync(email);
            return account != null ? MapToDto(account) : null;
        }

        public async Task<SystemAccount> UpdateAsync(SystemAccount entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                var existingAccount = await _unitOfWork.SystemAccounts.GetByIdAsync(
                    entity.AccountId
                );
                if (existingAccount == null)
                    throw new InvalidOperationException(
                        $"Không tìm thấy tài khoản với ID {entity.AccountId}"
                    );

                if (existingAccount.AccountEmail != entity.AccountEmail)
                {
                    ValidateEmail(entity.AccountEmail);
                    if (await IsEmailInUseAsync(entity.AccountEmail, entity.AccountId))
                        throw new InvalidOperationException("Email đã được sử dụng");
                }

                entity.ModifiedDate = DateTime.UtcNow;

                _unitOfWork.SystemAccounts.Update(entity);
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

        public async Task<SystemAccountDto> CreateAccountAsync(CreateSystemAccountDto createDto)
        {
            var account = new SystemAccount
            {
                AccountName = createDto.AccountName.Trim(),
                AccountEmail = createDto.AccountEmail.Trim().ToLower(),
                AccountPassword = createDto.AccountPassword,
                AccountRole = createDto.AccountRole,
                IsActive = true,
            };

            var createdAccount = await AddAsync(account);
            return MapToDto(createdAccount);
        }

        public async Task<SystemAccountDto> UpdateAccountAsync(
            int id,
            UpdateSystemAccountDto updateDto
        )
        {
            var existingAccount = await GetByIdAsync(id);
            if (existingAccount == null)
                throw new InvalidOperationException("Không tìm thấy tài khoản");

            if (!string.IsNullOrEmpty(updateDto.AccountName))
                existingAccount.AccountName = updateDto.AccountName.Trim();

            if (!string.IsNullOrEmpty(updateDto.AccountEmail))
                existingAccount.AccountEmail = updateDto.AccountEmail.Trim().ToLower();

            if (updateDto.AccountRole.HasValue)
                existingAccount.AccountRole = updateDto.AccountRole.Value;

            if (updateDto.IsActive.HasValue)
                existingAccount.IsActive = updateDto.IsActive.Value;

            var updatedAccount = await UpdateAsync(existingAccount);
            return MapToDto(updatedAccount);
        }

        public async Task<SystemAccountDto> UpdateProfileAsync(int id, UpdateProfileDto updateDto)
        {
            var existingAccount = await GetByIdAsync(id);
            if (existingAccount == null)
                throw new InvalidOperationException("Không tìm thấy tài khoản");

            if (!string.IsNullOrEmpty(updateDto.AccountName))
                existingAccount.AccountName = updateDto.AccountName.Trim();

            if (!string.IsNullOrEmpty(updateDto.AccountEmail))
                existingAccount.AccountEmail = updateDto.AccountEmail.Trim().ToLower();

            var updatedAccount = await UpdateAsync(existingAccount);
            return MapToDto(updatedAccount);
        }

        public async Task<bool> ChangePasswordAsync(int id, string oldPassword, string newPassword)
        {
            var account = await GetByIdAsync(id);
            if (account == null)
                throw new InvalidOperationException("Không tìm thấy tài khoản");

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, account.AccountPassword))
                throw new InvalidOperationException("Mật khẩu cũ không đúng");

            account.AccountPassword = PasswordUtil.HashPassword(newPassword);
            account.ModifiedDate = DateTime.UtcNow;

            await UpdateAsync(account);
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int id, string newPassword)
        {
            var account = await GetByIdAsync(id);
            if (account == null)
                throw new InvalidOperationException("Không tìm thấy tài khoản");

            account.AccountPassword = PasswordUtil.HashPassword(newPassword);
            account.ModifiedDate = DateTime.UtcNow;

            await UpdateAsync(account);
            return true;
        }

        public async Task<SystemAccountDto> ToggleAccountStatusAsync(int id)
        {
            var account = await GetByIdAsync(id);
            if (account == null)
                throw new InvalidOperationException("Không tìm thấy tài khoản");

            account.IsActive = !account.IsActive;
            var updatedAccount = await UpdateAsync(account);
            return MapToDto(updatedAccount);
        }

        public async Task<AccountStatisticsDto> GetAccountStatisticsAsync()
        {
            var allAccounts = await GetAllAsync();

            var statistics = new AccountStatisticsDto
            {
                TotalAccounts = allAccounts.Count(),
                ActiveAccounts = allAccounts.Count(a => a.IsActive),
                InactiveAccounts = allAccounts.Count(a => !a.IsActive),
                AdminAccounts = allAccounts.Count(a => a.AccountRole == AccountRole.Admin),
                StaffAccounts = allAccounts.Count(a => a.AccountRole == AccountRole.Staff),
                LecturerAccounts = allAccounts.Count(a => a.AccountRole == AccountRole.Lecturer),
                RoleDistribution = allAccounts
                    .GroupBy(a => a.AccountRole)
                    .Select(g => new RoleDistributionDto
                    {
                        Role = g.Key.ToString(),
                        Count = g.Count(),
                    })
                    .ToList(),
            };

            return statistics;
        }

        public async Task<bool> IsEmailInUseAsync(string email, int? excludeAccountId = null)
        {
            ValidateEmail(email);

            var allAccounts = await GetAllAsync();
            return allAccounts.Any(a =>
                a.AccountEmail.Equals(email, StringComparison.OrdinalIgnoreCase)
                && (!excludeAccountId.HasValue || a.AccountId != excludeAccountId)
            );
        }

        private void ValidateEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address != email)
                    throw new ArgumentException("Email không hợp lệ");
            }
            catch
            {
                throw new ArgumentException("Email không hợp lệ");
            }
        }

        private static SystemAccountDto MapToDto(SystemAccount account)
        {
            return new SystemAccountDto
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                AccountEmail = account.AccountEmail,
                AccountRole = account.AccountRole,
                IsActive = account.IsActive,
                CreatedDate = account.CreatedDate,
                ModifiedDate = account.ModifiedDate,
            };
        }
    }
}
