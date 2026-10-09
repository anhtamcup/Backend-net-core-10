namespace S3.Gateway.Entities
{
    public enum PaymentTransactionStatus
    {
        PENDING = 1,
        PROCESSING = 2,
        SUCCESS = 3,
        FAILED = 4,
        EXPIRED = 5,
        CANCELLED = 6
    }

    public enum PartnerCallbackStatus
    {
        PENDING = 1,
        SUCCESS = 2,
        FAILED = 3
    }

    public class PaymentTransaction : EntityBase
    {
        public int? PartnerID { get; set; }
        public Partner? Partner { get; set; }

        public string Provider { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string ClientRequestID { get; set; } = string.Empty;
        public string GatewayRequestID { get; set; } = string.Empty;
        public string ProviderTransactionID { get; set; } = string.Empty;
        public string ProviderReferenceID { get; set; } = string.Empty;
        public int? CallbackRoutingID { get; set; }

        public string AccountNumber { get; set; } = string.Empty;
        public string BankID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string BillNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string QRType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;

        public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.PENDING;
        public string ProviderStatus { get; set; } = string.Empty;
        public string ProviderCode { get; set; } = string.Empty;
        public string ProviderMessage { get; set; } = string.Empty;
        public DateTime? PaidAt { get; set; }
        public DateTime? LastNotificationAt { get; set; }
        public PartnerCallbackStatus PartnerCallbackStatus { get; set; } = PartnerCallbackStatus.PENDING;
        public int? PartnerCallbackHttpStatus { get; set; }
        public string PartnerCallbackCode { get; set; } = string.Empty;
        public string PartnerCallbackMessage { get; set; } = string.Empty;
        public DateTime? PartnerCallbackAt { get; set; }
        public string PartnerCallbackError { get; set; } = string.Empty;
        public string RawCreateRequest { get; set; } = string.Empty;
    }
}
