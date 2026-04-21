using S3.Gateway.Common;

namespace S3.Gateway.Entities
{
    public class RequestLog: EntityBase
    {
        public string RequestID { get; set; } = RequestContext.RequestID;

        public string Path { get; set; } = string.Empty;

        public string Method { get; set; } = string.Empty;

        public string RequestBody { get; set; } = string.Empty;

        public string ResponseBody { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public int Duration { get; set; }

        public string IpAddress { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public string StackTrace { get; set; } = string.Empty;
    }
}
