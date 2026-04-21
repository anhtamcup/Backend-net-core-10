using S3.Gateway.Common;

namespace S3.Gateway.Entities
{
    public class ApiLog: EntityBase
    {
        public string Method { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string RequestBody { get; set; } = string.Empty;

        public string ResponseBody { get; set; } = string.Empty;

        public int StatusCode { get; set; }

        public int Duration { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public string RequestID { get; set; } = RequestContext.RequestID;
    }
}
