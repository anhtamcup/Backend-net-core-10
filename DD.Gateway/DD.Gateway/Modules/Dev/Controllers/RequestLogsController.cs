using DD.Gateway.Data;
using DD.Gateway.Entities;
using DD.Gateway.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DD.Gateway.Modules.Dev.Controllers;

[ApiController]
[Route("api/dev/request-logs")]
[Authorize(Policy = PartnerScopes.DevRequestLogsRead)]
public sealed class RequestLogsController(GatewayDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(RequestLogSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RequestLogSearchResponse>> Search(
        [FromQuery] string? requestId,
        [FromQuery] string? path,
        [FromQuery] string? bodyContains,
        [FromQuery] string? method,
        [FromQuery] int? statusCode,
        [FromQuery] bool? hasError,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100
            || (long)(page - 1) * pageSize > int.MaxValue)
        {
            return BadRequest(new { message = "page phải từ 1; pageSize phải trong khoảng 1–100." });
        }

        if (statusCode is < 100 or > 599)
        {
            return BadRequest(new { message = "statusCode phải trong khoảng 100–599." });
        }

        if (fromUtc > toUtc)
        {
            return BadRequest(new { message = "fromUtc phải nhỏ hơn hoặc bằng toUtc." });
        }

        IQueryable<RequestLog> query = dbContext.RequestLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            query = query.Where(log => log.RequestId.Contains(requestId.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(path))
        {
            query = query.Where(log => log.Path.Contains(path.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(bodyContains))
        {
            var term = bodyContains.Trim();
            query = query.Where(log => log.RequestBody.Contains(term)
                || log.ResponseBody.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(method))
        {
            query = query.Where(log => log.Method == method.Trim().ToUpperInvariant());
        }

        if (statusCode is not null)
        {
            query = query.Where(log => log.StatusCode == statusCode);
        }

        if (hasError == true)
        {
            query = query.Where(log => log.StatusCode >= 400 || log.ErrorMessage != "");
        }
        else if (hasError == false)
        {
            query = query.Where(log => log.StatusCode < 400 && log.ErrorMessage == "");
        }

        if (fromUtc is not null)
        {
            var from = fromUtc.Value.UtcDateTime;
            query = query.Where(log => log.CreatedAt >= from);
        }

        if (toUtc is not null)
        {
            var to = toUtc.Value.UtcDateTime;
            query = query.Where(log => log.CreatedAt <= to);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(log => log.CreatedAt)
            .ThenByDescending(log => log.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(log => new RequestLogItem(
                log.Id,
                log.CreatedAt,
                log.RequestId,
                log.Path,
                log.Method,
                log.RequestBody,
                log.ResponseBody,
                log.StatusCode,
                log.Duration,
                log.IpAddress,
                log.ErrorMessage,
                log.StackTrace,
                log.RequestHeader))
            .ToListAsync(cancellationToken);

        return Ok(new RequestLogSearchResponse(items, totalCount, page, pageSize));
    }

    public sealed record RequestLogItem(
        long Id,
        DateTime CreatedAt,
        string RequestId,
        string Path,
        string Method,
        string RequestBody,
        string ResponseBody,
        int StatusCode,
        int Duration,
        string IpAddress,
        string ErrorMessage,
        string StackTrace,
        string RequestHeader);

    public sealed record RequestLogSearchResponse(
        IReadOnlyList<RequestLogItem> Items,
        int TotalCount,
        int Page,
        int PageSize);
}
