using System.Net;
using System.Text.Json;
using BusinessObject.Common;
using Microsoft.AspNetCore.Mvc;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Set content type
            context.Response.ContentType = "application/json";

            // Get request ID for tracking
            var requestId = context.TraceIdentifier;

            // Determine response based on exception type
            var response = exception switch
            {
                UnauthorizedAccessException => ApiResponse<object>.UnauthorizedResponse(exception.Message),
                ArgumentNullException argNullEx => ApiResponse<object>.ValidationErrorResponse(
                    new { Field = argNullEx.ParamName, Message = argNullEx.Message }, 
                    "Tham số không hợp lệ"),
                ArgumentException => ApiResponse<object>.ValidationErrorResponse(
                    new { Message = exception.Message }, 
                    "Dữ liệu không hợp lệ"),
                KeyNotFoundException => ApiResponse<object>.NotFoundResponse(exception.Message),
                InvalidOperationException => ApiResponse<object>.ErrorResponse(exception.Message, 400),
                NotImplementedException => ApiResponse<object>.ErrorResponse("Tính năng chưa được triển khai", 501),
                TimeoutException => ApiResponse<object>.ErrorResponse("Yêu cầu hết thời gian chờ", 408),
                _ => ApiResponse<object>.ErrorResponse("Có lỗi xảy ra trong quá trình xử lý", 500)
            };

            // Set status code
            context.Response.StatusCode = response.StatusCode;

            // Set request ID
            response.RequestId = requestId;

            // Add detailed error info in development environment
            if (_environment.IsDevelopment())
            {
                response.Errors = new
                {
                    Type = exception.GetType().Name,
                    Message = exception.Message,
                    StackTrace = exception.StackTrace,
                    InnerException = exception.InnerException?.Message
                };
            }

            // Serialize and write response
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
} 