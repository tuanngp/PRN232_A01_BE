using BusinessObject;
using BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using NguyenGiaPhuongTuan_SE17D06_A01_BE.DTOs;
using Services;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SystemAccountController : BaseController
    {
        private readonly ISystemAccountService _systemAccountService;

        public SystemAccountController(ISystemAccountService systemAccountService)
        {
            _systemAccountService = systemAccountService;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<IActionResult> GetSystemAccounts()
        {
            try
            {
                var accounts = await _systemAccountService.GetAllAsync();

                var accountsResponse = accounts
                    .Select(a => new SystemAccountResponseDto
                    {
                        AccountId = a.AccountId,
                        AccountName = a.AccountName,
                        AccountEmail = a.AccountEmail,
                        AccountRole = a.AccountRole,
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedDate,
                        UpdatedAt = a.ModifiedDate,
                    })
                    .ToList();

                return Success(accountsResponse, "Lấy danh sách tài khoản thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy danh sách tài khoản: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSystemAccount(int id)
        {
            try
            {
                var account = await _systemAccountService.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                var accountResponse = new SystemAccountResponseDto
                {
                    AccountId = account.AccountId,
                    AccountName = account.AccountName,
                    AccountEmail = account.AccountEmail,
                    AccountRole = account.AccountRole,
                    IsActive = account.IsActive,
                    CreatedAt = account.CreatedDate,
                    UpdatedAt = account.ModifiedDate,
                };

                return Success(accountResponse, "Lấy thông tin tài khoản thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thông tin tài khoản: {ex.Message}");
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            try
            {
                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var account = await _systemAccountService.GetByIdAsync(currentUserId.Value);
                if (account == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                var accountResponse = new SystemAccountResponseDto
                {
                    AccountId = account.AccountId,
                    AccountName = account.AccountName,
                    AccountEmail = account.AccountEmail,
                    AccountRole = account.AccountRole,
                    IsActive = account.IsActive,
                    CreatedAt = account.CreatedDate,
                    UpdatedAt = account.ModifiedDate,
                };

                return Success(accountResponse, "Lấy thông tin profile thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thông tin profile: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSystemAccount(
            [FromBody] CreateSystemAccountDto createDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var allAccounts = await _systemAccountService.GetAllAsync();
                var existingAccount = allAccounts.FirstOrDefault(a =>
                    a.AccountEmail.Equals(
                        createDto.AccountEmail,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                if (existingAccount != null)
                {
                    return ValidationError(
                        new { AccountEmail = new[] { "Email đã được sử dụng" } }
                    );
                }

                var account = new SystemAccount
                {
                    AccountName = createDto.AccountName.Trim(),
                    AccountEmail = createDto.AccountEmail.Trim().ToLower(),
                    AccountPassword = createDto.AccountPassword,
                    AccountRole = createDto.AccountRole,
                    IsActive = true,
                };

                var createdAccount = await _systemAccountService.AddAsync(account);

                var accountResponse = new SystemAccountResponseDto
                {
                    AccountId = createdAccount.AccountId,
                    AccountName = createdAccount.AccountName,
                    AccountEmail = createdAccount.AccountEmail,
                    AccountRole = createdAccount.AccountRole,
                    IsActive = createdAccount.IsActive,
                    CreatedAt = createdAccount.CreatedDate,
                    UpdatedAt = createdAccount.ModifiedDate,
                };

                return Created(accountResponse, "Tạo tài khoản thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi tạo tài khoản: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSystemAccount(
            int id,
            [FromBody] UpdateSystemAccountDto updateDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var existingAccount = await _systemAccountService.GetByIdAsync(id);
                if (existingAccount == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                if (
                    !string.IsNullOrEmpty(updateDto.AccountEmail)
                    && !updateDto.AccountEmail.Equals(
                        existingAccount.AccountEmail,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    var allAccounts = await _systemAccountService.GetAllAsync();
                    var duplicateAccount = allAccounts.FirstOrDefault(a =>
                        a.AccountId != id
                        && a.AccountEmail.Equals(
                            updateDto.AccountEmail,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

                    if (duplicateAccount != null)
                    {
                        return ValidationError(
                            new { AccountEmail = new[] { "Email đã được sử dụng" } }
                        );
                    }

                    existingAccount.AccountEmail = updateDto.AccountEmail.Trim().ToLower();
                }

                if (!string.IsNullOrEmpty(updateDto.AccountName))
                    existingAccount.AccountName = updateDto.AccountName.Trim();

                if (updateDto.AccountRole.HasValue)
                    existingAccount.AccountRole = updateDto.AccountRole.Value;

                if (updateDto.IsActive.HasValue)
                    existingAccount.IsActive = updateDto.IsActive.Value;

                var updatedAccount = await _systemAccountService.UpdateAsync(existingAccount);

                var accountResponse = new SystemAccountResponseDto
                {
                    AccountId = updatedAccount.AccountId,
                    AccountName = updatedAccount.AccountName,
                    AccountEmail = updatedAccount.AccountEmail,
                    AccountRole = updatedAccount.AccountRole,
                    IsActive = updatedAccount.IsActive,
                    CreatedAt = updatedAccount.CreatedDate,
                    UpdatedAt = updatedAccount.ModifiedDate,
                };

                return Success(accountResponse, "Cập nhật tài khoản thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật tài khoản: {ex.Message}");
            }
        }

        [HttpPut("profile")]
        [Authorize] // Tất cả người dùng có thể cập nhật profile của mình
        public async Task<IActionResult> UpdateCurrentUserProfile(
            [FromBody] UpdateProfileDto updateDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var existingAccount = await _systemAccountService.GetByIdAsync(currentUserId.Value);
                if (existingAccount == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                // Kiểm tra email trùng lặp (nếu có thay đổi)
                if (
                    !string.IsNullOrEmpty(updateDto.AccountEmail)
                    && !updateDto.AccountEmail.Equals(
                        existingAccount.AccountEmail,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    var allAccounts = await _systemAccountService.GetAllAsync();
                    var duplicateAccount = allAccounts.FirstOrDefault(a =>
                        a.AccountId != currentUserId
                        && a.AccountEmail.Equals(
                            updateDto.AccountEmail,
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

                    if (duplicateAccount != null)
                    {
                        return ValidationError(
                            new { AccountEmail = new[] { "Email đã được sử dụng" } }
                        );
                    }

                    existingAccount.AccountEmail = updateDto.AccountEmail.Trim().ToLower();
                }

                // Cập nhật tên (người dùng có thể tự cập nhật tên)
                if (!string.IsNullOrEmpty(updateDto.AccountName))
                    existingAccount.AccountName = updateDto.AccountName.Trim();

                var updatedAccount = await _systemAccountService.UpdateAsync(existingAccount);

                var accountResponse = new SystemAccountResponseDto
                {
                    AccountId = updatedAccount.AccountId,
                    AccountName = updatedAccount.AccountName,
                    AccountEmail = updatedAccount.AccountEmail,
                    AccountRole = updatedAccount.AccountRole,
                    IsActive = updatedAccount.IsActive,
                    CreatedAt = updatedAccount.CreatedDate,
                    UpdatedAt = updatedAccount.ModifiedDate,
                };

                return Success(accountResponse, "Cập nhật profile thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật profile: {ex.Message}");
            }
        }

        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto changePasswordDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var (currentUserId, _, _) = GetCurrentUser();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                var account = await _systemAccountService.GetByIdAsync(currentUserId.Value);
                if (account == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                // Xác thực mật khẩu cũ
                if (
                    !BCrypt.Net.BCrypt.Verify(
                        changePasswordDto.OldPassword,
                        account.AccountPassword
                    )
                )
                {
                    return ValidationError(
                        new { OldPassword = new[] { "Mật khẩu cũ không đúng" } }
                    );
                }

                await _systemAccountService.UpdateAsync(account);

                return Success("Đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi đổi mật khẩu: {ex.Message}");
            }
        }

        [HttpPatch("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto resetDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return ValidationError(ModelState);
                }

                var account = await _systemAccountService.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                await _systemAccountService.UpdateAsync(account);

                return Success("Reset mật khẩu thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi reset mật khẩu: {ex.Message}");
            }
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleAccountStatus(int id)
        {
            try
            {
                var account = await _systemAccountService.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                account.IsActive = !account.IsActive;
                var updatedAccount = await _systemAccountService.UpdateAsync(account);

                var accountResponse = new SystemAccountResponseDto
                {
                    AccountId = updatedAccount.AccountId,
                    AccountName = updatedAccount.AccountName,
                    AccountEmail = updatedAccount.AccountEmail,
                    AccountRole = updatedAccount.AccountRole,
                    IsActive = updatedAccount.IsActive,
                    CreatedAt = updatedAccount.CreatedDate,
                    UpdatedAt = updatedAccount.ModifiedDate,
                };

                string message = account.IsActive
                    ? "Kích hoạt tài khoản thành công"
                    : "Tạm dừng tài khoản thành công";
                return Success(accountResponse, message);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi thay đổi trạng thái tài khoản: {ex.Message}");
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetAccountStatistics()
        {
            try
            {
                var allAccounts = await _systemAccountService.GetAllAsync();

                var statistics = new
                {
                    TotalAccounts = allAccounts.Count(),
                    ActiveAccounts = allAccounts.Count(a => a.IsActive),
                    InactiveAccounts = allAccounts.Count(a => !a.IsActive),
                    AdminAccounts = allAccounts.Count(a => a.AccountRole == AccountRole.Admin),
                    StaffAccounts = allAccounts.Count(a => a.AccountRole == AccountRole.Staff),
                    LecturerAccounts = allAccounts.Count(a =>
                        a.AccountRole == AccountRole.Lecturer
                    ),
                    RoleDistribution = allAccounts
                        .GroupBy(a => a.AccountRole)
                        .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
                        .ToList(),
                };

                return Success(statistics, "Lấy thống kê tài khoản thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thống kê tài khoản: {ex.Message}");
            }
        }
    }
}
