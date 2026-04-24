namespace S3.Gateway.Integrations.eKYC.Napas
{
    public class NapasConfig
    {
        public NapasKeyConfig Key { get; set; } = new();
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

    public class NapasKeyConfig
    {
        public NapasEkycKeyConfig eKYC { get; set; } = new();
    }

    public class NapasEkycKeyConfig
    {
        public string PrivateKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
