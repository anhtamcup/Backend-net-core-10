namespace DD.Gateway.Entities;

public sealed class EkycSession
{
    public string SessionId { get; set; } = string.Empty;
    public int PartnerId { get; set; }
    public Partner Partner { get; set; } = null!;
    public string Provider { get; set; } = "NAMABANK";
    public string ProviderPartnerId { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public long IssuedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? ResultHash { get; set; }
    public string? Status { get; set; }
    public string? Code { get; set; }
}
