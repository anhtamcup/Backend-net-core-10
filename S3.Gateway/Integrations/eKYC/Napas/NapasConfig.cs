namespace S3.Gateway.Integrations.Ekyc.Napas
{
    public class NapasConfig
    {
        public EkycObject Ekyc { get; set; } = new();
        public PaymentObject Payment { get; set; } = new();
        public string OauthURL { get; set; } = string.Empty;
        public string GetDeepLinkURL { get; set; } = string.Empty;
        public string CreateMerchantURL { get; set; } = string.Empty;
        public string UpdateMerchantURL { get; set; } = string.Empty;
        public string QueryMerchantURL { get; set; } = string.Empty;
        public string UpdateBankAccountURL { get; set; } = string.Empty;
        public string GrantType { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string ForwardNotiPaymentURL { get; set; } = string.Empty;
    }

    public class EkycObject
    {
        public string PrivateKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
    }

    public class PaymentObject
    {
        public string PrivateKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string SSL { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
