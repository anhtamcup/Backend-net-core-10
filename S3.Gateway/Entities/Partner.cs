namespace S3.Gateway.Entities
{
    public enum PartnerStatus
    {
        ACTIVE = 1,
        DISABLED = 2
    }

    public class Partner : EntityBase
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PartnerStatus Status { get; set; } = PartnerStatus.ACTIVE;
        public ICollection<PartnerCredential> Credentials { get; set; } = new List<PartnerCredential>();
    }
}
