using Newtonsoft.Json;

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

    public class NpOAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }
    }

    public class NpOriginalDataResponse
    {
        public string RefId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string OrigDate { get; set; } = string.Empty;
        public decimal OrigAmount { get; set; }
    }

    public class NpRefundDataResponse
    {
        public string RefundId { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
    }

    public class SettlementInfoResponse
    {
        public string SettlementDate { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public int settlementCode { get; set; }
    }

    public class NpRefundResponse
    {
        public string CaseId { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public NpOriginalDataResponse OriginalData { get; set; } = new NpOriginalDataResponse();
        public NpRefundDataResponse RefundData { get; set; } = new NpRefundDataResponse();
        public SettlementInfoResponse? SettlementInfo { get; set; }
    }
}
