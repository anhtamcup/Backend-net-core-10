namespace DD.Gateway.Entities;

public enum PartnerCredentialStatus
{
    Active = 1,
    Revoked = 2,
    Expired = 3
}

public sealed class PartnerCredential
{
    public int Id { get; set; }
    public int PartnerId { get; set; }
    public Partner Partner { get; set; } = null!;
    public string AccessKey { get; set; } = string.Empty;
    public string? SecretKey { get; set; }
    public string SecretKeyHash { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
    public PartnerCredentialStatus Status { get; set; } = PartnerCredentialStatus.Active;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
