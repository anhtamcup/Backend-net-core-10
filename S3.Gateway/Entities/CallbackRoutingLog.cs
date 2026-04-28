using S3.Gateway.Common;

namespace S3.Gateway.Entities
{
    public class CallbackRoutingLog: EntityBase
    {
        public string RequestID { get; set; } = RequestContext.RequestID;
        public string RequestPayload { get; set; } = string.Empty;
        public string ResponsePayload { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
