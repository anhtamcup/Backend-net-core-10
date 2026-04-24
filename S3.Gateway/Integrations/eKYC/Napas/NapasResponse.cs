using Newtonsoft.Json;

namespace S3.Gateway.Integrations.eKYC.Napas
{
    public class NpResponseInfo
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class NpOAuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [JsonProperty("token_type")]
        public string TokenType { get; set; } = string.Empty;
        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; } = string.Empty;
    }

    public class NpGetDeepLinkResponse
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public NpResponseInfo Response { get; set; } = new();
    }

    public class NpCallBackStatusResponse
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public NpResponseInfo Response { get; set; } = new();
    }

    public class NpCreateMerchantResponse
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string MerchantCode { get; set; } = string.Empty;
        public string MerchantAlias { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<BranchObject> Branch { get; set; } = new();
        public NpResponseInfo Response { get; set; } = new();

        public class BranchObject
        {
            public string PlatformBranchId { get; set; } = string.Empty;
            public string BranchName { get; set; } = string.Empty;
            public string BranchCode { get; set; } = string.Empty;
            public string QrName { get; set; } = string.Empty;
        }
    }

    public class NpUpdateMerchantResponse
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string BranchAlias { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantAddress { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string QrName { get; set; } = string.Empty;
        public string PaymentChannel { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public NpResponseInfo Response { get; set; } = new();
    }

    public class NpQueryMerchantResponse
    {
        public string SenderReference { get; set; } = string.Empty;
        public DateTime CreationDateTime { get; set; }
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string MerchantName { get; set; } = string.Empty;
        public string MerchantAddress { get; set; } = string.Empty;
        public string MerchantCode { get; set; } = string.Empty;
        public string MerchantAlias { get; set; } = string.Empty;
        public string MerchantType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<BranchInfo> Branch { get; set; } = new();
        public string QrName { get; set; } = string.Empty;
        public string PaymentChannel { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        public string Mcc { get; set; } = string.Empty;
        public decimal Fee { get; set; }
        public decimal MasterMerchantFee { get; set; }
        public NpResponseInfo Response { get; set; } = new();

        public class BranchInfo
        {
            public string PlatformBranchId { get; set; } = string.Empty;
            public string BranchName { get; set; } = string.Empty;
            public string BranchCode { get; set; } = string.Empty;
            public string BranchAccount { get; set; } = string.Empty;
            public string BranchBankId { get; set; } = string.Empty;
            public string BeneficiaryName { get; set; } = string.Empty;
        }
    }

    public class NpUpdateBankAccountResponse
    {
        public string SenderReference { get; set; } = string.Empty;
        public string CreationDateTime { get; set; } = string.Empty;
        public string PlatformCode { get; set; } = string.Empty;
        public string PlatformMerchantId { get; set; } = string.Empty;
        public string BranchAlias { get; set; } = string.Empty;
        public string BranchAccount { get; set; } = string.Empty;
        public string BranchBankId { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public NpResponseInfo Response { get; set; } = new();
    }

    public class NpPaymentNotificationResponse
    {
        public string Code { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}