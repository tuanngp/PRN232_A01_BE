using System.Text.Json.Serialization;

namespace BusinessObject.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Errors { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ResponseMetadata? Metadata { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RequestId { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Thành công", ResponseMetadata? metadata = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = 200,
                Message = message,
                Data = data,
                Metadata = metadata
            };
        }

        public static ApiResponse<T> CreatedResponse(T data, string message = "Tạo thành công")
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = 201,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = errors
            };
        }

        public static ApiResponse<T> NotFoundResponse(string message = "Không tìm thấy dữ liệu")
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = 404,
                Message = message
            };
        }

        public static ApiResponse<T> UnauthorizedResponse(string message = "Không có quyền truy cập")
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = 401,
                Message = message
            };
        }

        public static ApiResponse<T> ForbiddenResponse(string message = "Truy cập bị từ chối")
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = 403,
                Message = message
            };
        }

        public static ApiResponse<T> ValidationErrorResponse(object errors, string message = "Dữ liệu không hợp lệ")
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = 422,
                Message = message,
                Errors = errors
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public new static ApiResponse SuccessResponse(string message = "Thành công")
        {
            return new ApiResponse
            {
                Success = true,
                StatusCode = 200,
                Message = message
            };
        }

        public new static ApiResponse ErrorResponse(string message, int statusCode = 400, object? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                Errors = errors
            };
        }
    }

    public class ResponseMetadata
    {
        [JsonPropertyName("@odata.context")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ODataContext { get; set; }

        [JsonPropertyName("@odata.count")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? ODataCount { get; set; }

        [JsonPropertyName("@odata.nextLink")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ODataNextLink { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PaginationInfo? Pagination { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object>? Additional { get; set; }
    }

    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
} 