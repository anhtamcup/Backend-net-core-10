namespace DD.Gateway.Security;

public static class PartnerScopes
{
    public const string EkycDeepLinkCreate = "ekyc.deeplink.create";
    public const string DevRequestLogsRead = "dev.requestlogs.read";
    public const string DevDatabaseRead = "dev.database.read";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        EkycDeepLinkCreate
    };

    public static readonly IReadOnlySet<string> Assignable = new HashSet<string>(
        All.Append(DevRequestLogsRead).Append(DevDatabaseRead),
        StringComparer.Ordinal);
}
