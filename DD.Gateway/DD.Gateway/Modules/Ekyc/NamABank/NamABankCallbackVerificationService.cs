using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using DD.Gateway.Modules.Ekyc.Providers;

namespace DD.Gateway.Modules.Ekyc.NamABank;

public sealed class NamABankCallbackVerificationService(
    IOptions<NamABankEkycOptions> options,
    IHostEnvironment hostEnvironment)
{
    public EkycVerifiedResult Verify(string? result)
    {
        var settings = options.Value;
        settings.ValidateCallbackSettings();

        if (string.IsNullOrWhiteSpace(result) || result.Length > settings.MaximumCallbackResultLength)
        {
            throw new NamABankEkycRequestException("result JWS không hợp lệ.");
        }

        var parts = result.Split('.');
        if (parts.Length != 3 || parts.Any(part => part.Length == 0 ||
                part.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not ('-' or '_'))))
        {
            throw new NamABankEkycRequestException("result phải là JWS Compact không có padding.");
        }

        byte[] headerBytes;
        byte[] payloadBytes;
        byte[] signature;
        try
        {
            headerBytes = WebEncoders.Base64UrlDecode(parts[0]);
            payloadBytes = WebEncoders.Base64UrlDecode(parts[1]);
            signature = WebEncoders.Base64UrlDecode(parts[2]);
        }
        catch (Exception error) when (error is FormatException or ArgumentException)
        {
            throw new NamABankEkycRequestException("result chứa Base64URL không hợp lệ.");
        }

        if (WebEncoders.Base64UrlEncode(headerBytes) != parts[0]
            || WebEncoders.Base64UrlEncode(payloadBytes) != parts[1]
            || WebEncoders.Base64UrlEncode(signature) != parts[2])
        {
            throw new NamABankEkycRequestException("result chứa Base64URL không chuẩn.");
        }

        try
        {
            using var header = JsonDocument.Parse(headerBytes);
            var algorithm = RequiredString(header.RootElement, "alg");
            var keyId = RequiredString(header.RootElement, "kid");
            if (algorithm != "RS256")
            {
                throw new NamABankEkycRequestException("JWS chỉ chấp nhận thuật toán RS256.");
            }

            if (string.IsNullOrWhiteSpace(settings.Kid))
            {
                throw new NamABankEkycConfigurationException(
                    "Chưa cấu hình kid khóa ký callback do NAB cấp.");
            }

            if (keyId != settings.Kid)
            {
                throw new NamABankEkycRequestException("kid callback NAB không được chấp nhận.");
            }

            var keyPath = settings.CallbackPublicKeyPath;
            if (string.IsNullOrWhiteSpace(keyPath))
            {
                throw new NamABankEkycConfigurationException(
                    "Đường dẫn public key xác minh callback NAB chưa được cấu hình.");
            }

            string pem;
            try
            {
                var fullPath = Path.GetFullPath(keyPath, hostEnvironment.ContentRootPath);
                pem = File.ReadAllText(fullPath);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException)
            {
                throw new NamABankEkycConfigurationException("Không thể đọc public key xác minh callback NAB.", error);
            }

            using var rsa = RSA.Create();
            try
            {
                rsa.ImportFromPem(pem);
            }
            catch (Exception error) when (error is CryptographicException or ArgumentException)
            {
                throw new NamABankEkycConfigurationException("Public key xác minh callback NAB không hợp lệ.", error);
            }

            if (rsa.KeySize < 2048)
            {
                throw new NamABankEkycConfigurationException("Public key xác minh callback NAB phải dài ít nhất 2048 bit.");
            }

            var signingInput = Encoding.ASCII.GetBytes(parts[0] + "." + parts[1]);
            if (signature.Length != rsa.KeySize / 8
                || !rsa.VerifyData(signingInput, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            {
                throw new NamABankEkycRequestException("Chữ ký callback NAB không hợp lệ.");
            }

            using var payload = JsonDocument.Parse(payloadBytes);
            var sessionId = RequiredString(payload.RootElement, "sessionID");
            var status = RequiredString(payload.RootElement, "status");
            var code = RequiredString(payload.RootElement, "code");
            if (!Guid.TryParseExact(sessionId, "D", out _)
                || status != "SUCCESS" && status != "FAILED"
                || status == "SUCCESS" && code != "00"
                || status == "FAILED" && code is not ("01" or "02" or "03" or "04" or "05" or "06" or "07" or "08"))
            {
                throw new NamABankEkycRequestException("Nội dung callback NAB không hợp lệ.");
            }

            return new EkycVerifiedResult(
                sessionId, status, code,
                OptionalString(payload.RootElement, "messageVi"),
                OptionalString(payload.RootElement, "messageEn"));
        }
        catch (JsonException)
        {
            throw new NamABankEkycRequestException("JSON trong callback NAB không hợp lệ.");
        }
    }

    private static string RequiredString(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new NamABankEkycRequestException("JWS header/payload phải là JSON object.");
        }

        var matches = element.EnumerateObject().Where(property => property.NameEquals(name)).ToArray();
        if (matches.Length != 1 || matches[0].Value.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(matches[0].Value.GetString()))
        {
            throw new NamABankEkycRequestException($"Trường {name} trong callback NAB không hợp lệ.");
        }

        return matches[0].Value.GetString()!;
    }

    private static string? OptionalString(JsonElement element, string name)
    {
        var matches = element.EnumerateObject().Where(property => property.NameEquals(name)).ToArray();
        if (matches.Length > 1 || matches.Length == 1 && matches[0].Value.ValueKind != JsonValueKind.String)
        {
            throw new NamABankEkycRequestException($"Trường {name} trong callback NAB không hợp lệ.");
        }

        return matches.Length == 1 ? matches[0].Value.GetString() : null;
    }
}
