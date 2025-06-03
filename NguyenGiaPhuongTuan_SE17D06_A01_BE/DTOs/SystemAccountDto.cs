using System.ComponentModel.DataAnnotations;
using BusinessObject.Enums;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.DTOs
{
    public class SystemAccountResponseDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountEmail { get; set; } = string.Empty;
        public AccountRole AccountRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateSystemAccountDto
    {
        [Required(ErrorMessage = "Tên tài khoản là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên tài khoản không được vượt quá 100 ký tự")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string AccountEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [StringLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự")]
        public string AccountPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public AccountRole AccountRole { get; set; }
    }

    public class UpdateSystemAccountDto
    {
        [StringLength(100, ErrorMessage = "Tên tài khoản không được vượt quá 100 ký tự")]
        public string? AccountName { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? AccountEmail { get; set; }

        public AccountRole? AccountRole { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UpdateProfileDto
    {
        [StringLength(100, ErrorMessage = "Tên tài khoản không được vượt quá 100 ký tự")]
        public string? AccountName { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        public string? AccountEmail { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [StringLength(100, ErrorMessage = "Mật khẩu mới không được vượt quá 100 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
        [StringLength(100, ErrorMessage = "Mật khẩu mới không được vượt quá 100 ký tự")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
