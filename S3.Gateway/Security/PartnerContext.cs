using System.Security.Claims;

namespace S3.Gateway.Security
{
    public interface IPartnerContext
    {
        int PartnerID { get; }
        int CredentialID { get; }
        string PartnerCode { get; }
    }

    public class PartnerContext : IPartnerContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PartnerContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int PartnerID => GetIntClaim(PartnerAccessKeyDefaults.PartnerIdClaim);
        public int CredentialID => GetIntClaim(PartnerAccessKeyDefaults.CredentialIdClaim);
        public string PartnerCode => GetClaim(PartnerAccessKeyDefaults.PartnerCodeClaim);

        private int GetIntClaim(string type)
        {
            return int.TryParse(GetClaim(type), out var value) ? value : 0;
        }

        private string GetClaim(string type)
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(type) ?? string.Empty;
        }
    }
}
