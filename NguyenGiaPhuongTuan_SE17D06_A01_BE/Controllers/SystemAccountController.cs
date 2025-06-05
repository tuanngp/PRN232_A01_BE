using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Services.DTOs;
using Services.Interface;

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
                //var accounts = await _systemAccountService.GetAllAccountsAsync();
                //return Success(accounts, "Lấy danh sách tài khoản thành công");
                var accounts = await _systemAccountService.GetAllAccountsAsync();
                return Ok(accounts);
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
                var account = await _systemAccountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                return Success(account, "Lấy thông tin tài khoản thành công");
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

                var account = await _systemAccountService.GetAccountByIdAsync(currentUserId.Value);
                if (account == null)
                {
                    return NotFound("Không tìm thấy tài khoản");
                }

                return Success(account, "Lấy thông tin profile thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thông tin profile: {ex.Message}");
            }
        }

        [HttpPost]
        [AllowAnonymous]
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

                var createdAccount = await _systemAccountService.CreateAccountAsync(createDto);
                return Created(createdAccount, "Tạo tài khoản thành công");
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new { Message = ex.Message });
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

                var updatedAccount = await _systemAccountService.UpdateAccountAsync(id, updateDto);
                return Success(updatedAccount, "Cập nhật tài khoản thành công");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi cập nhật tài khoản: {ex.Message}");
            }
        }

        [HttpPut("profile")]
        [Authorize]
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

                var updatedAccount = await _systemAccountService.UpdateProfileAsync(
                    currentUserId.Value,
                    updateDto
                );
                return Success(updatedAccount, "Cập nhật profile thành công");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
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

                var result = await _systemAccountService.ChangePasswordAsync(
                    currentUserId.Value,
                    changePasswordDto.OldPassword,
                    changePasswordDto.NewPassword
                );

                return Success("Đổi mật khẩu thành công");
            }
            catch (InvalidOperationException ex)
            {
                return ValidationError(new { Message = ex.Message });
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

                var result = await _systemAccountService.ResetPasswordAsync(
                    id,
                    resetDto.NewPassword
                );
                return Success("Reset mật khẩu thành công");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
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
                var updatedAccount = await _systemAccountService.ToggleAccountStatusAsync(id);
                string message = updatedAccount.IsActive
                    ? "Kích hoạt tài khoản thành công"
                    : "Tạm dừng tài khoản thành công";
                return Success(updatedAccount, message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
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
                var statistics = await _systemAccountService.GetAccountStatisticsAsync();
                return Success(statistics, "Lấy thống kê tài khoản thành công");
            }
            catch (Exception ex)
            {
                return Error($"Lỗi khi lấy thống kê tài khoản: {ex.Message}");
            }
        }
    }
}
