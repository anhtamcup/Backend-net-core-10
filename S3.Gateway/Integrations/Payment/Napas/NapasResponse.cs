namespace S3.Gateway.Integrations.Payment.Napas
{
    public class NpPaymentNotificationResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

    public class NpInvestigationResponse
    {
        public string CaseId { get; set; } = string.Empty;

        public string CreationDateTime { get; set; } = string.Empty;

        public string Id { get; set; } = string.Empty;

        public string TransDateTime { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string IssueBank { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string BeneficiaryBank { get; set; } = string.Empty;

        public string RealMerchantAccount { get; set; } = string.Empty;

        public string Mcc { get; set; } = string.Empty;

        public string SourceAccount { get; set; } = string.Empty;

        public string SystemTrace { get; set; } = string.Empty;

        public string LocalTime { get; set; } = string.Empty;

        public string LocalDate { get; set; } = string.Empty;

        public string TerminalId { get; set; } = string.Empty;

        public string RefId { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}
