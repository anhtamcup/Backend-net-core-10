namespace S3.Gateway.Security
{
    public static class PartnerAccessKeyDefaults
    {
        public const string AuthenticationScheme = "PartnerAccessKey";
        public const string AccessKeyHeader = "X-Access-Key";
        public const string SecretKeyHeader = "X-Secret-Key";
        public const string PartnerIdClaim = "partner_id";
        public const string PartnerCodeClaim = "partner_code";
        public const string CredentialIdClaim = "credential_id";
        public const string ScopeClaim = "scope";
    }
}
