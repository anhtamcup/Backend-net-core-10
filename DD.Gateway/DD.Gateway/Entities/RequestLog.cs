namespace DD.Gateway.Entities;

public sealed class RequestLog
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string RequestBody { get; set; } = string.Empty;
    public string ResponseBody { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int Duration { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public string RequestHeader { get; set; } = string.Empty;
}
