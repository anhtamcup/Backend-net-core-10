namespace S3.Gateway.Entities
{
    public enum PartnerCredentialStatus
    {
        ACTIVE = 1,
        REVOKED = 2,
        EXPIRED = 3
    }

    public class PartnerCredential : EntityBase
    {
        public int PartnerID { get; set; }
        public Partner Partner { get; set; } = null!;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKeyHash { get; set; } = string.Empty;
        public string Scopes { get; set; } = string.Empty;
        public PartnerCredentialStatus Status { get; set; } = PartnerCredentialStatus.ACTIVE;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
