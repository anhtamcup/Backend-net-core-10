namespace DD.Gateway.Entities;

public enum PartnerStatus
{
    Active = 1,
    Disabled = 2
}

public sealed class Partner
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CallbackUrl { get; set; }
    public PartnerStatus Status { get; set; } = PartnerStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PartnerCredential> Credentials { get; set; } = new List<PartnerCredential>();
}
