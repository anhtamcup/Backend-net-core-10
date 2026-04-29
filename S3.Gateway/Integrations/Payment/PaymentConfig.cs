namespace S3.Gateway.Integrations.Payment
{
    public class PaymentConfig
    {
        public NapasConfig Napas { get; set; } = new();

        public class NapasConfig
        {
            public string PrivateKey { get; set; } = string.Empty;
            public string PublicKey { get; set; } = string.Empty;
            public string Application { get; set; } = string.Empty;

            public string OauthURL { get; set; } = string.Empty;
            public string InvestigationURL { get; set; } = string.Empty;

            public string GrantType { get; set; } = string.Empty;
            public string ClientId { get; set; } = string.Empty;
            public string ClientSecret { get; set; } = string.Empty;

            public string SenderId { get; set; } = string.Empty;
            public string ReceiverId { get; set; } = string.Empty;

            public string ForwardNotiPaymentURL { get; set; } = string.Empty;

            public string SSL { get; set; } = string.Empty;
            public string SSLPassword { get; set; } = string.Empty;
        }
    }
}
