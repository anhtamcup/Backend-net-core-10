using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DD.Gateway.Data;
using DD.Gateway.Entities;

namespace DD.Gateway.Middleware;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private const int MaxBodyBytes = 64 * 1024;

    public async Task InvokeAsync(HttpContext context, GatewayDbContext dbContext)
    {
        var startedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var requestBody = await ReadRequestBodyAsync(context.Request);
        var responseBody = context.Response.Body;
        using var capture = new CapturingWriteStream(responseBody, MaxBodyBytes);
        context.Response.Body = capture;

        Exception? requestError = null;
        try
        {
            await next(context);
        }
        catch (Exception error)
        {
            requestError = error;
            throw;
        }
        finally
        {
            context.Response.Body = responseBody;
            stopwatch.Stop();

            var log = new RequestLog
            {
                CreatedAt = startedAt,
                RequestId = context.TraceIdentifier,
                Path = context.Request.Path.Value is { } path
                    ? path[..Math.Min(path.Length, 2048)]
                    : string.Empty,
                Method = context.Request.Method,
                RequestBody = requestBody,
                ResponseBody = IsTextContent(context.Response.ContentType)
                    ? capture.CapturedText
                    : "[non-text response omitted]",
                StatusCode = requestError is null ? context.Response.StatusCode : StatusCodes.Status500InternalServerError,
                Duration = (int)Math.Min(stopwatch.ElapsedMilliseconds, int.MaxValue),
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                ErrorMessage = requestError?.Message ?? string.Empty,
                StackTrace = requestError?.ToString() ?? string.Empty,
                RequestHeader = JsonSerializer.Serialize(context.Request.Headers.ToDictionary(
                    header => header.Key,
                    header => header.Value.ToString()))
            };

            try
            {
                dbContext.RequestLogs.Add(log);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception error)
            {
                logger.LogError(error, "Could not save request log {RequestId}", context.TraceIdentifier);
            }
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        if (!IsTextContent(request.ContentType))
        {
            return request.ContentLength is > 0 ? "[non-text request omitted]" : string.Empty;
        }

        request.EnableBuffering();
        var buffer = new byte[MaxBodyBytes + 1];
        var bytesRead = 0;

        try
        {
            while (bytesRead < buffer.Length)
            {
                var count = await request.Body.ReadAsync(buffer.AsMemory(bytesRead));
                if (count == 0)
                {
                    break;
                }

                bytesRead += count;
            }
        }
        finally
        {
            request.Body.Position = 0;
        }

        var truncated = bytesRead > MaxBodyBytes;
        return Encoding.UTF8.GetString(buffer, 0, Math.Min(bytesRead, MaxBodyBytes))
            + (truncated ? " [truncated]" : string.Empty);
    }

    private static bool IsTextContent(string? contentType) =>
        contentType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) == true
        || contentType?.Contains("json", StringComparison.OrdinalIgnoreCase) == true
        || contentType?.Contains("xml", StringComparison.OrdinalIgnoreCase) == true
        || contentType?.Contains("x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) == true;

    private sealed class CapturingWriteStream(Stream destination, int maxBytes) : Stream
    {
        private readonly MemoryStream _capture = new();
        private bool _truncated;

        public string CapturedText => Encoding.UTF8.GetString(_capture.ToArray())
            + (_truncated ? " [truncated]" : string.Empty);

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => destination.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => destination.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            Capture(buffer.AsSpan(offset, count));
            destination.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            Capture(buffer);
            destination.Write(buffer);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            Capture(buffer.AsSpan(offset, count));
            await destination.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Capture(buffer.Span);
            return destination.WriteAsync(buffer, cancellationToken);
        }

        private void Capture(ReadOnlySpan<byte> buffer)
        {
            var remaining = maxBytes - (int)_capture.Length;
            if (remaining > 0)
            {
                _capture.Write(buffer[..Math.Min(remaining, buffer.Length)]);
            }

            _truncated |= buffer.Length > remaining;
        }
    }
}
