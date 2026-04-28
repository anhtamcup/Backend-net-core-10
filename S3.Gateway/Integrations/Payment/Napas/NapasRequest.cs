namespace S3.Gateway.Integrations.Payment.Napas
{
    public class NpRequestBase<T>
    {
        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NpHeader Header { get; set; } = new();

        public T Payload { get; set; }
    }

    public class NpPaymentNotificationRequest
    {
        public string Status { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string TransDateTime { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string IssueBank { get; set; } = string.Empty;
        public string BeneficiaryBank { get; set; } = string.Empty;
        public string RealMerchantAccount { get; set; } = string.Empty;
        public string Mcc { get; set; } = string.Empty;
        public string SourceAccount { get; set; } = string.Empty;
        public string SystemTrace { get; set; } = string.Empty;
        public string LocalTime { get; set; } = string.Empty;
        public string LocalDate { get; set; } = string.Empty;
        public string TerminalId { get; set; } = string.Empty;
        public string RefId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public NpSettlementInfo SettlementInfo { get; set; } = new();
        public string CaseId { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
    }

    public class NpSettlementInfo
    {
        public string SettlementDate { get; set; } = string.Empty;
        public int SettlementCode { get; set; }
    }

    public class NpHeader
    {
        public string MessageIdentifier { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string SenderReference { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
    }

    public class NpInvestigationRequest
    {
        public string CaseId { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string TransDateTime { get; set; } = string.Empty;
    }
}
