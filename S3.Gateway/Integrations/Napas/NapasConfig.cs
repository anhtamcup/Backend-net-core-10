namespace S3.Gateway.Integrations.Napas
{
    public class NapasConfig
    {
        public string OauthURL { get; set; } = string.Empty;
        public string GetDeepLinkURL { get; set; } = string.Empty;
        public string CreateMerchantURL { get; set; } = string.Empty;
        public string UpdateMerchantURL { get; set; } = string.Empty;
        public string QueryMerchantURL { get; set; } = string.Empty;
        public string UpdateBankAccountURL { get; set; } = string.Empty;
        public string GrantType { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}
