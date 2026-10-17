using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace DD.Gateway.Modules.Ekyc.NamABank;

public sealed class NamABankDeepLinkService(
    IOptions<NamABankEkycOptions> options,
    TimeProvider timeProvider,
    IHostEnvironment hostEnvironment)
{
    public NamABankDeepLinkResult Create(string? partnerId, string? callbackUrl)
    {
        var settings = options.Value;
        settings.ValidateDeepLinkSettings();

        if (string.IsNullOrWhiteSpace(partnerId) || partnerId.Length > 100)
        {
            throw new NamABankEkycRequestException("partnerID phải có 1-100 ký tự.");
        }

        if (string.IsNullOrWhiteSpace(callbackUrl)
            || !Uri.TryCreate(callbackUrl, UriKind.Absolute, out var callbackUri)
            || callbackUri.Scheme is "http" or "https"
            || string.IsNullOrWhiteSpace(callbackUri.Host)
            || string.IsNullOrWhiteSpace(callbackUri.AbsolutePath)
            || callbackUri.Fragment.Length != 0)
        {
            throw new NamABankEkycRequestException(
                "callbackUrl phải là custom URL scheme đã đăng ký với NamABank.");
        }

        if (QueryHelpers.ParseQuery(callbackUri.Query).Keys.Any(
                name => name.Equals("result", StringComparison.OrdinalIgnoreCase)))
        {
            throw new NamABankEkycRequestException(
                "callbackUrl không được chứa sẵn query parameter result.");
        }

        var sessionId = Guid.NewGuid().ToString("D");
        var issuedAt = timeProvider.GetUtcNow().ToUnixTimeSeconds();
        var plaintext = JsonSerializer.SerializeToUtf8Bytes(new
        {
            partnerID = partnerId,
            sessionID = sessionId,
            callbackUrl,
            issuedAt
        });

        if (plaintext.Length < 1 || plaintext.Length > settings.MaximumPlaintextBytes)
        {
            throw new NamABankEkycRequestException(
                $"Payload eKYC dài {plaintext.Length} byte UTF-8; NamABank chỉ nhận tối đa {settings.MaximumPlaintextBytes} byte.");
        }

        var publicKeyContents = ReadPublicKeyFile();

        byte[] ciphertext;
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyContents);
            if (rsa.KeySize != settings.RsaKeySizeBits)
            {
                throw new NamABankEkycConfigurationException($"Public key NamABank phải là RSA-{settings.RsaKeySizeBits}.");
            }

            ciphertext = rsa.Encrypt(plaintext, RSAEncryptionPadding.OaepSHA256);
        }
        catch (CryptographicException error)
        {
            throw new NamABankEkycConfigurationException("Public key NamABank không hợp lệ.", error);
        }
        catch (ArgumentException error)
        {
            throw new NamABankEkycConfigurationException("Public key NamABank không hợp lệ.", error);
        }

        if (ciphertext.Length != settings.CiphertextBytes)
        {
            throw new NamABankEkycConfigurationException($"Ciphertext NamABank phải dài đúng {settings.CiphertextBytes} byte.");
        }

        var data = WebEncoders.Base64UrlEncode(ciphertext);
        if (data.Length != settings.EncodedCiphertextLength
            || data.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_')))
        {
            throw new NamABankEkycConfigurationException("Ciphertext Base64URL không hợp lệ.");
        }

        var url = QueryHelpers.AddQueryString(settings.BaseUrl, "data", data);
        return new NamABankDeepLinkResult(sessionId, issuedAt, url);
    }

    private string ReadPublicKeyFile()
    {
        if (string.IsNullOrWhiteSpace(options.Value.PublicKeyPath))
        {
            throw new NamABankEkycConfigurationException("Thiếu đường dẫn public key mã hóa yêu cầu NAB.");
        }

        try
        {
            var path = Path.GetFullPath(options.Value.PublicKeyPath, hostEnvironment.ContentRootPath);
            return File.ReadAllText(path);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException)
        {
            throw new NamABankEkycConfigurationException("Không thể đọc public key NamABank.", error);
        }
    }
}

public sealed record NamABankDeepLinkResult(string SessionId, long IssuedAt, string Url);

public sealed class NamABankEkycRequestException(string message) : Exception(message);

public sealed class NamABankEkycConfigurationException(string message, Exception? innerException = null)
    : Exception(message, innerException);
