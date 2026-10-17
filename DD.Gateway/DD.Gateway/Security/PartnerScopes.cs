namespace DD.Gateway.Security;

public static class PartnerScopes
{
    public const string EkycDeepLinkCreate = "ekyc.deeplink.create";
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        EkycDeepLinkCreate
    };
}
