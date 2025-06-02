using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Controllers
{
    [Route("api/[controller]")]
    public class TestController : BaseController
    {

        [HttpGet("public")]
        [AllowAnonymous]
        public IActionResult PublicEndpoint()
        {
            return Success(new { timestamp = DateTime.UtcNow }, "Đây là endpoint public, không cần đăng nhập.");
        }

        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedEndpoint()
        {
            var (userId, email, role) = GetCurrentUser();

            return Success(new { userId, email, role }, "Đây là endpoint được bảo vệ, cần đăng nhập.");
        }

        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnlyEndpoint()
        {
            var (userId, email, role) = GetCurrentUser();
            return Success(new { userId, email, role }, "Đây là endpoint chỉ dành cho Admin.");
        }

        [HttpGet("staff-admin")]
        [Authorize(Roles = "Staff,Admin")]
        public IActionResult StaffAdminEndpoint()
        {
            var (userId, email, role) = GetCurrentUser();
            return Success(new { userId, email, role }, "Đây là endpoint dành cho Staff và Admin.");
        }

        [HttpGet("admin-policy")]
        [Authorize(Policy = "AdminPolicy")]
        public IActionResult AdminPolicyEndpoint()
        {
            var (userId, email, role) = GetCurrentUser();
            return Success(new { userId, email, role }, "Đây là endpoint sử dụng AdminPolicy.");
        }

        [HttpGet("test-exception")]
        [AllowAnonymous]
        public IActionResult TestException()
        {
            throw new InvalidOperationException("Đây là test exception để kiểm tra middleware xử lý lỗi.");
        }

        [HttpPost("test-validation")]
        [AllowAnonymous]
        public IActionResult TestValidation([FromBody] TestModel model)
        {
            if (!ModelState.IsValid)
            {
                return ValidationError(ModelState, "Dữ liệu test không hợp lệ.");
            }

            return Success(model, "Validation test thành công.");
        }

        [HttpGet("test-notfound")]
        [AllowAnonymous]
        public IActionResult TestNotFound()
        {
            return NotFound("Test resource không tồn tại.");
        }
    }

    public class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
} 