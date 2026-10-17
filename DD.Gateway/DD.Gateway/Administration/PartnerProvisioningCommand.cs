using System.Globalization;
using DD.Gateway.Data;
using DD.Gateway.Entities;
using DD.Gateway.Security;
using Microsoft.EntityFrameworkCore;

namespace DD.Gateway.Administration;

public static class PartnerProvisioningCommand
{
    public static bool CanHandle(string[] args) =>
        args.Length >= 2
        && (args[0].Equals("partner", StringComparison.OrdinalIgnoreCase)
            || args[0].Equals("credential", StringComparison.OrdinalIgnoreCase));

    public static async Task<int> RunAsync(IServiceProvider services, string[] args)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();

        try
        {
            if (Matches(args, "partner", "create"))
            {
                return await CreatePartnerAsync(dbContext, args);
            }

            if (Matches(args, "partner", "update"))
            {
                return await UpdatePartnerAsync(dbContext, args);
            }

            if (Matches(args, "credential", "create"))
            {
                return await CreateCredentialAsync(dbContext, args);
            }

            WriteUsage();
            return 1;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"Không thể thực hiện lệnh: {error.Message}");
            return 1;
        }
    }

    private static async Task<int> CreatePartnerAsync(GatewayDbContext dbContext, string[] args)
    {
        var code = GetOption(args, "--code")?.Trim().ToUpperInvariant();
        var name = GetOption(args, "--name")?.Trim();
        var callbackUrl = GetOption(args, "--callback-url")?.Trim();
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name)
            || code.Length > 50 || name.Length > 200
            || !code.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_'))
        {
            Console.Error.WriteLine("--code cần 1-50 ký tự A-Z, 0-9, '-' hoặc '_'; --name cần 1-200 ký tự.");
            return 1;
        }

        if (callbackUrl is not null && !IsValidCallbackUrl(callbackUrl))
        {
            Console.Error.WriteLine("--callback-url cần là URL HTTP/HTTPS đầy đủ, tối đa 2048 ký tự.");
            return 1;
        }

        if (await dbContext.Partners.AnyAsync(item => item.Code == code))
        {
            Console.Error.WriteLine($"Partner '{code}' đã tồn tại.");
            return 1;
        }

        var partner = new Partner { Code = code, Name = name, CallbackUrl = callbackUrl };
        dbContext.Partners.Add(partner);
        await dbContext.SaveChangesAsync();
        Console.WriteLine($"Đã tạo partner: ID={partner.Id}, Code={partner.Code}, Name={partner.Name}, CallbackUrl={partner.CallbackUrl ?? "Chưa cấu hình"}");
        return 0;
    }

    private static async Task<int> UpdatePartnerAsync(GatewayDbContext dbContext, string[] args)
    {
        var code = GetOption(args, "--code")?.Trim().ToUpperInvariant();
        var callbackUrl = GetOption(args, "--callback-url")?.Trim();
        if (string.IsNullOrWhiteSpace(code) || callbackUrl is null || !IsValidCallbackUrl(callbackUrl))
        {
            Console.Error.WriteLine("Cần --code <CODE> và --callback-url <URL HTTP/HTTPS đầy đủ, tối đa 2048 ký tự>.");
            return 1;
        }

        var partner = await dbContext.Partners.SingleOrDefaultAsync(item => item.Code == code);
        if (partner is null)
        {
            Console.Error.WriteLine($"Không tìm thấy partner '{code}'.");
            return 1;
        }

        partner.CallbackUrl = callbackUrl;
        partner.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        Console.WriteLine($"Đã cập nhật callback URL cho partner {partner.Code}: {partner.CallbackUrl}");
        return 0;
    }

    private static async Task<int> CreateCredentialAsync(GatewayDbContext dbContext, string[] args)
    {
        var partnerCode = GetOption(args, "--partner")?.Trim().ToUpperInvariant();
        var environment = GetOption(args, "--environment")?.Trim().ToLowerInvariant();
        var scopesValue = GetOption(args, "--scopes")?.Trim();
        var expiresValue = GetOption(args, "--expires")?.Trim();
        if (string.IsNullOrWhiteSpace(partnerCode) || environment is not ("uat" or "prod"))
        {
            Console.Error.WriteLine("Cần --partner <CODE> và --environment <uat|prod>.");
            return 1;
        }

        var scopes = string.IsNullOrWhiteSpace(scopesValue)
            || scopesValue.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? PartnerScopes.All.OrderBy(item => item).ToArray()
            : scopesValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.Ordinal).OrderBy(item => item).ToArray();
        var invalidScopes = scopes.Where(item => !PartnerScopes.All.Contains(item)).ToArray();
        if (scopes.Length == 0 || invalidScopes.Length > 0)
        {
            Console.Error.WriteLine(invalidScopes.Length > 0
                ? $"Scope không hợp lệ: {string.Join(", ", invalidScopes)}"
                : "Phải cấp ít nhất một scope.");
            return 1;
        }

        DateTime? expiresAt = null;
        if (!string.IsNullOrWhiteSpace(expiresValue))
        {
            if (!DateTime.TryParseExact(expiresValue, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var expiresDate))
            {
                Console.Error.WriteLine("--expires phải theo định dạng yyyy-MM-dd.");
                return 1;
            }

            expiresAt = DateTime.SpecifyKind(expiresDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
        }

        var partner = await dbContext.Partners.SingleOrDefaultAsync(item => item.Code == partnerCode);
        if (partner is null || partner.Status != PartnerStatus.Active)
        {
            Console.Error.WriteLine($"Không tìm thấy partner đang hoạt động '{partnerCode}'.");
            return 1;
        }

        string accessKey;
        do
        {
            accessKey = $"dd_{environment}_{PartnerCredentialSecret.Generate(16)}";
        }
        while (await dbContext.PartnerCredentials.AnyAsync(item => item.AccessKey == accessKey));

        var secretKey = PartnerCredentialSecret.Generate();
        var credential = new PartnerCredential
        {
            PartnerId = partner.Id,
            AccessKey = accessKey,
            SecretKey = secretKey,
            SecretKeyHash = PartnerCredentialSecret.Hash(secretKey),
            Scopes = string.Join(',', scopes),
            ExpiresAt = expiresAt
        };

        dbContext.PartnerCredentials.Add(credential);
        await dbContext.SaveChangesAsync();

        Console.WriteLine("Đã tạo credential. Secret key được lưu trong database.");
        Console.WriteLine($"Partner: {partner.Code}");
        Console.WriteLine($"Access key: {accessKey}");
        Console.WriteLine($"Secret key: {secretKey}");
        Console.WriteLine($"Scopes: {credential.Scopes}");
        Console.WriteLine($"Expires at: {(expiresAt.HasValue ? expiresAt.Value.ToString("yyyy-MM-dd") : "Không hết hạn")}");
        return 0;
    }

    private static bool Matches(string[] args, string resource, string action) =>
        args[0].Equals(resource, StringComparison.OrdinalIgnoreCase)
        && args[1].Equals(action, StringComparison.OrdinalIgnoreCase);

    private static bool IsValidCallbackUrl(string value) =>
        value.Length <= 2048
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https"
        && !string.IsNullOrWhiteSpace(uri.Host);

    private static string? GetOption(string[] args, string name)
    {
        for (var index = 2; index < args.Length - 1; index++)
        {
            if (args[index].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return args[index + 1];
            }
        }

        return null;
    }

    private static void WriteUsage()
    {
        Console.WriteLine("Cách dùng:");
        Console.WriteLine("  partner create --code <CODE> --name <NAME> [--callback-url <URL>]");
        Console.WriteLine("  partner update --code <CODE> --callback-url <URL>");
        Console.WriteLine("  credential create --partner <CODE> --environment <uat|prod> [--scopes <scope1,scope2|all>] [--expires yyyy-MM-dd]");
    }
}
