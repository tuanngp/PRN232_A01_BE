using BusinessObject;
using BusinessObject.Enums;
using Services.DTOs;

namespace Services.Interface
{
    public interface ISystemAccountService : IBaseService<SystemAccount>
    {
        Task<IEnumerable<SystemAccountDto>> GetAllAccountsAsync();
        Task<SystemAccountDto?> GetAccountByIdAsync(int id);
        Task<SystemAccountDto?> GetAccountByEmailAsync(string email);
        Task<SystemAccountDto> CreateAccountAsync(CreateSystemAccountDto createDto);
        Task<SystemAccountDto> UpdateAccountAsync(int id, UpdateSystemAccountDto updateDto);
        Task<SystemAccountDto> UpdateProfileAsync(int id, UpdateProfileDto updateDto);
        Task<bool> ChangePasswordAsync(int id, string oldPassword, string newPassword);
        Task<bool> ResetPasswordAsync(int id, string newPassword);
        Task<SystemAccountDto> ToggleAccountStatusAsync(int id);
        Task<AccountStatisticsDto> GetAccountStatisticsAsync();
        Task<bool> IsEmailInUseAsync(string email, int? excludeAccountId = null);
    }
}
