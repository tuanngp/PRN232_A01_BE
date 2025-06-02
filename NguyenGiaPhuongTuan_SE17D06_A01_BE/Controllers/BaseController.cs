using System.Security.Claims;
using BusinessObject.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult Success<T>(
            T data,
            string message = "Thành công",
            ResponseMetadata? metadata = null
        )
        {
            var response = ApiResponse<T>.SuccessResponse(data, message, metadata);
            response.RequestId = HttpContext.TraceIdentifier;
            return Ok(response);
        }

        protected IActionResult Success(string message = "Thành công")
        {
            var response = ApiResponse.SuccessResponse(message);
            response.RequestId = HttpContext.TraceIdentifier;
            return Ok(response);
        }

        protected IActionResult Created<T>(T data, string message = "Tạo thành công")
        {
            var response = ApiResponse<T>.CreatedResponse(data, message);
            response.RequestId = HttpContext.TraceIdentifier;
            return StatusCode(201, response);
        }

        protected IActionResult NotFound(string message = "Không tìm thấy dữ liệu")
        {
            var response = ApiResponse<object>.NotFoundResponse(message);
            response.RequestId = HttpContext.TraceIdentifier;
            return StatusCode(404, response);
        }

        protected IActionResult ValidationError(
            ModelStateDictionary modelState,
            string message = "Dữ liệu không hợp lệ"
        )
        {
            var errors = modelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var response = ApiResponse<object>.ValidationErrorResponse(errors, message);
            response.RequestId = HttpContext.TraceIdentifier;
            return StatusCode(422, response);
        }

        protected IActionResult ValidationError(
            object errors,
            string message = "Dữ liệu không hợp lệ"
        )
        {
            var response = ApiResponse<object>.ValidationErrorResponse(errors, message);
            response.RequestId = HttpContext.TraceIdentifier;
            return StatusCode(422, response);
        }

        protected IActionResult Error(string message, int statusCode = 400, object? errors = null)
        {
            var response = ApiResponse<object>.ErrorResponse(message, statusCode, errors);
            response.RequestId = HttpContext.TraceIdentifier;
            return StatusCode(statusCode, response);
        }

        protected IActionResult Unauthorized(string message = "Không có quyền truy cập")
        {
            var response = ApiResponse<object>.UnauthorizedResponse(message);
            response.RequestId = HttpContext.TraceIdentifier;
            return StatusCode(401, response);
        }

        protected IActionResult Forbidden(string message = "Truy cập bị từ chối")
        {
            var response = ApiResponse<object>.ForbiddenResponse(message);
            response.RequestId = HttpContext.TraceIdentifier;
            return StatusCode(403, response);
        }

        protected ResponseMetadata CreatePaginationMetadata<T>(
            IQueryable<T> query,
            int page,
            int pageSize,
            long totalCount
        )
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new ResponseMetadata
            {
                Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1,
                },
            };
        }

        protected (int? UserId, string? Email, string? Role) GetCurrentUser()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            int? userId = null;
            if (int.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            return (userId, email, role);
        }

        protected bool HasRole(string role)
        {
            return User.IsInRole(role);
        }

        protected bool CanAccessResource(int resourceOwnerId)
        {
            var (currentUserId, _, _) = GetCurrentUser();
            return HasRole("Admin") || currentUserId == resourceOwnerId;
        }
    }
}
