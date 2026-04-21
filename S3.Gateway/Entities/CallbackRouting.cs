namespace S3.Gateway.Entities
{
    public enum Target3rd
    {
        Napas = 1,
    }

    public enum CallbackRoutingStatus
    {
        INPROCESS = 1,
        COMPLETED = 2,
        ERROR = 3
    }

    public class CallbackRouting: EntityBase
    {
        public string RequestID { get; set; } = string.Empty;
        public Guid RefID { get; set; } = new Guid();
        public Target3rd Target { get; set; }
        public string PartnerCode { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public string ActionHistory { get; set; } = string.Empty;

        public string RequestPayload { get; set; } = string.Empty;
        public string ResponsePayload { get; set; } = string.Empty;

        public CallbackRoutingStatus Status { get; set; } = CallbackRoutingStatus.INPROCESS;
        public string StatusString { get; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
