using S3.Gateway.Common;
using S3.Gateway.Entities;
using S3.Gateway.Features.Logs;

namespace S3.Gateway.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            RequestContext.RequestID = context.TraceIdentifier;

            var logService = context.RequestServices
                .GetRequiredService<ILogService>();

            var start = DateTime.Now;
            context.Request.EnableBuffering();
            string requestBody = string.Empty;
            using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var originalBody = context.Response.Body;

            using var memStream = new MemoryStream();
            context.Response.Body = memStream;
            string responseBody = string.Empty;
            int statusCode = 200;
            string errorMessage = string.Empty;
            string stackTrace = string.Empty;

            try
            {
                await _next(context);
                memStream.Position = 0;
                responseBody = await new StreamReader(memStream).ReadToEndAsync();
                memStream.Position = 0;
                await memStream.CopyToAsync(originalBody);
                statusCode = context.Response.StatusCode;
            }
            catch (Exception ex)
            {
                statusCode = 500;
                errorMessage = ex.Message;
                stackTrace = ex.ToString();
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Lỗi hệ thống"
                });
            }
            finally
            {
                var duration = (int)(DateTime.Now - start).TotalMilliseconds;
                await logService.SaveAsync(new RequestLog
                {
                    Path = context.Request.Path,
                    Method = context.Request.Method,
                    RequestBody = requestBody,
                    ResponseBody = responseBody,
                    StatusCode = statusCode,
                    Duration = duration,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                    ErrorMessage = errorMessage,
                    StackTrace = stackTrace
                });
            }
        }
    }
}