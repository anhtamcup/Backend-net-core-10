using DD.Gateway.Modules.Ekyc.NamABank;

namespace DD.Gateway.Modules.Ekyc.Providers;

public sealed class NamABankEkycProvider(
    NamABankDeepLinkService deepLinkService,
    NamABankCallbackVerificationService callbackVerificationService) : IEkycProvider
{
    public string Name => "NAMABANK";

    public EkycDeepLinkResult CreateDeepLink(string? partnerId, string? callbackUrl)
    {
        try
        {
            var result = deepLinkService.Create(partnerId, callbackUrl);
            return new EkycDeepLinkResult(Name, result.SessionId, result.IssuedAt, result.Url);
        }
        catch (NamABankEkycRequestException error)
        {
            throw new EkycProviderRequestException(error.Message, error);
        }
        catch (NamABankEkycConfigurationException error)
        {
            throw new EkycProviderConfigurationException(error.Message, error);
        }
    }

    public EkycVerifiedResult VerifyCallback(string? result)
    {
        try
        {
            return callbackVerificationService.Verify(result);
        }
        catch (NamABankEkycRequestException error)
        {
            throw new EkycProviderRequestException(error.Message, error);
        }
        catch (NamABankEkycConfigurationException error)
        {
            throw new EkycProviderConfigurationException(error.Message, error);
        }
    }
}
