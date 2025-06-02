using System.Text;
using System.Text.Json;
using BusinessObject.Common;

namespace NguyenGiaPhuongTuan_SE17D06_A01_BE.Middleware
{
    public class ODataResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ODataResponseMiddleware> _logger;

        public ODataResponseMiddleware(RequestDelegate next, ILogger<ODataResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if this is an OData request
            if (IsODataRequest(context))
            {
                await HandleODataRequest(context);
            }
            else
            {
                await _next(context);
            }
        }

        private static bool IsODataRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/odata") ||
                   context.Request.Query.ContainsKey("$select") ||
                   context.Request.Query.ContainsKey("$filter") ||
                   context.Request.Query.ContainsKey("$expand") ||
                   context.Request.Query.ContainsKey("$orderby") ||
                   context.Request.Query.ContainsKey("$top") ||
                   context.Request.Query.ContainsKey("$skip") ||
                   context.Request.Query.ContainsKey("$count");
        }

        private async Task HandleODataRequest(HttpContext context)
        {
            // Capture the original response body
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                // Check if response was successful
                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    await WrapODataResponse(context, responseBody, originalBodyStream);
                }
                else
                {
                    // For error responses, copy as-is
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task WrapODataResponse(HttpContext context, MemoryStream responseBody, Stream originalBodyStream)
        {
            try
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseContent = await new StreamReader(responseBody).ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(responseContent))
                {
                    // Empty response
                    var emptyResponse = ApiResponse<object>.SuccessResponse(null, "Thành công");
                    emptyResponse.RequestId = context.TraceIdentifier;
                    await WriteResponse(originalBodyStream, emptyResponse);
                    return;
                }

                // Parse OData response
                var odataResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                // Extract OData metadata
                var metadata = ExtractODataMetadata(odataResponse);

                // Create wrapped response
                var wrappedResponse = ApiResponse<object>.SuccessResponse(
                    odataResponse, 
                    "Thành công", 
                    metadata);

                wrappedResponse.RequestId = context.TraceIdentifier;

                // Write wrapped response
                await WriteResponse(originalBodyStream, wrappedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error wrapping OData response");
                
                // Fallback: return original response
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private static ResponseMetadata? ExtractODataMetadata(JsonElement odataResponse)
        {
            var metadata = new ResponseMetadata();
            bool hasMetadata = false;

            if (odataResponse.TryGetProperty("@odata.context", out var contextElement))
            {
                metadata.ODataContext = contextElement.GetString();
                hasMetadata = true;
            }

            if (odataResponse.TryGetProperty("@odata.count", out var countElement))
            {
                if (countElement.TryGetInt64(out var count))
                {
                    metadata.ODataCount = count;
                    hasMetadata = true;
                }
            }

            if (odataResponse.TryGetProperty("@odata.nextLink", out var nextLinkElement))
            {
                metadata.ODataNextLink = nextLinkElement.GetString();
                hasMetadata = true;
            }

            return hasMetadata ? metadata : null;
        }

        private static async Task WriteResponse(Stream stream, ApiResponse<object> response)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(response, jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(bytes);
        }
    }

    public static class ODataResponseMiddlewareExtensions
    {
        public static IApplicationBuilder UseODataResponseWrapper(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ODataResponseMiddleware>();
        }
    }
} 