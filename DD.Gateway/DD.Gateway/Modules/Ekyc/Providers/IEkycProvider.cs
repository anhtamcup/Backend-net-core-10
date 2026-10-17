namespace DD.Gateway.Modules.Ekyc.Providers;

public interface IEkycProvider
{
    string Name { get; }

    EkycDeepLinkResult CreateDeepLink(string? partnerId, string? callbackUrl);

    EkycVerifiedResult VerifyCallback(string? result);
}

public sealed record EkycDeepLinkResult(string Provider, string SessionId, long IssuedAt, string Url);
public sealed record EkycVerifiedResult(
    string SessionId,
    string Status,
    string Code,
    string? MessageVi,
    string? MessageEn);

public sealed class EkycProviderRegistry(IEnumerable<IEkycProvider> providers)
{
    private readonly IReadOnlyDictionary<string, IEkycProvider> _providers = providers
        .ToDictionary(provider => provider.Name, StringComparer.OrdinalIgnoreCase);

    public IEkycProvider GetRequired(string? name)
    {
        if (!string.IsNullOrWhiteSpace(name)
            && _providers.TryGetValue(name.Trim(), out var provider))
        {
            return provider;
        }

        throw new UnsupportedEkycProviderException(name);
    }
}

public sealed class UnsupportedEkycProviderException(string? name)
    : Exception($"Provider eKYC '{name}' chưa được hỗ trợ.");

public sealed class EkycProviderRequestException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class EkycProviderConfigurationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
