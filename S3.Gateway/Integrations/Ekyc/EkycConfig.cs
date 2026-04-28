namespace S3.Gateway.Integrations.Ekyc
{
    public class EkycConfig
    {
        public NapasConfig Napas { get; set; } = new();

        public class NapasConfig
        {
            public string PrivateKey { get; set; } = string.Empty;
            public string PublicKey { get; set; } = string.Empty;
            public string Application { get; set; } = string.Empty;

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
}
