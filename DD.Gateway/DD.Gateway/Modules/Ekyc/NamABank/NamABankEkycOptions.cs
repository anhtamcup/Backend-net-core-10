namespace DD.Gateway.Modules.Ekyc.NamABank;

public sealed class NamABankEkycOptions
{
    public const string SectionName = "NamABank:Ekyc";

    public string BaseUrl { get; set; } = string.Empty;
    public int MaximumPlaintextBytes { get; set; }
    public int RsaKeySizeBits { get; set; }
    public int CiphertextBytes { get; set; }
    public int EncodedCiphertextLength { get; set; }
    public int MaximumCallbackResultLength { get; set; }
    public string PublicKeyPath { get; set; } = string.Empty;
    public string Kid { get; set; } = string.Empty;
    public string CallbackPublicKeyPath { get; set; } = string.Empty;

    public void ValidateDeepLinkSettings()
    {
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps
            || string.IsNullOrWhiteSpace(uri.Host)
            || uri.Query.Length != 0
            || uri.Fragment.Length != 0
            || uri.UserInfo.Length != 0)
        {
            throw new NamABankEkycConfigurationException("BaseUrl NAB phải là URL HTTPS không có query hoặc fragment.");
        }

        if (RsaKeySizeBits != 2048
            || CiphertextBytes != RsaKeySizeBits / 8
            || EncodedCiphertextLength != (CiphertextBytes * 8 + 5) / 6
            || MaximumPlaintextBytes is < 1 or > 190)
        {
            throw new NamABankEkycConfigurationException("Giới hạn payload và RSA NAB trong appsettings không đúng đặc tả.");
        }
    }

    public void ValidateCallbackSettings()
    {
        if (MaximumCallbackResultLength is < 1 or > 65536)
        {
            throw new NamABankEkycConfigurationException("MaximumCallbackResultLength phải từ 1 đến 65536.");
        }
    }
}
