namespace S3.Gateway.Entities
{
    public enum PaymentNotificationProcessingStatus
    {
        RECEIVED = 1,
        PROCESSED = 2,
        DUPLICATE = 3,
        ERROR = 4
    }

    public class PaymentNotification : EntityBase
    {
        public int PaymentTransactionID { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; } = null!;

        // RequestID is the RequestLog correlation key. RequestLogs are persisted asynchronously.
        public string RequestID { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ProviderTransactionID { get; set; } = string.Empty;
        public string ProviderReferenceID { get; set; } = string.Empty;
        public string ProviderStatus { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public PaymentNotificationProcessingStatus ProcessingStatus { get; set; } = PaymentNotificationProcessingStatus.RECEIVED;
        public bool IsDuplicate { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.Now;
        public DateTime? ProcessedAt { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
