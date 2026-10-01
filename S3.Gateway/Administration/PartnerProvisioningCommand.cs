using System.Globalization;
using Microsoft.EntityFrameworkCore;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Security;

namespace S3.Gateway.Administration
{
    public static class PartnerProvisioningCommand
    {
        public static bool CanHandle(string[] args)
        {
            return args.Length >= 2 &&
                (args[0].Equals("partner", StringComparison.OrdinalIgnoreCase) ||
                 args[0].Equals("credential", StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<int> RunAsync(IServiceProvider services, string[] args)
        {
            await using var scope = services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

            try
            {
                if (Matches(args, "partner", "create"))
                    return await CreatePartnerAsync(dbContext, args);

                if (Matches(args, "credential", "create"))
                    return await CreateCredentialAsync(dbContext, args);

                WriteUsage();
                return 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Không thể thực hiện lệnh: {ex.Message}");
                return 1;
            }
        }

        private static async Task<int> CreatePartnerAsync(DBContext dbContext, string[] args)
        {
            var code = GetOption(args, "--code")?.Trim().ToUpperInvariant();
            var name = GetOption(args, "--name")?.Trim();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                Console.Error.WriteLine("Thiếu --code hoặc --name.");
                return 1;
            }

            if (code.Length > 50 || !code.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_'))
            {
                Console.Error.WriteLine("Partner code chỉ gồm A-Z, 0-9, '-' hoặc '_' và tối đa 50 ký tự.");
                return 1;
            }

            if (await dbContext.Partners.AnyAsync(item => item.Code == code))
            {
                Console.Error.WriteLine($"Partner '{code}' đã tồn tại.");
                return 1;
            }

            var partner = new Partner
            {
                Code = code,
                Name = name,
                Status = PartnerStatus.ACTIVE
            };

            dbContext.Partners.Add(partner);
            await dbContext.SaveChangesAsync();
            Console.WriteLine($"Đã tạo Partner: ID={partner.ID}, Code={partner.Code}, Name={partner.Name}");
            return 0;
        }

        private static async Task<int> CreateCredentialAsync(DBContext dbContext, string[] args)
        {
            var partnerCode = GetOption(args, "--partner")?.Trim().ToUpperInvariant();
            var environment = GetOption(args, "--environment")?.Trim().ToLowerInvariant();
            var scopesValue = GetOption(args, "--scopes")?.Trim();
            var expiresValue = GetOption(args, "--expires")?.Trim();

            if (string.IsNullOrWhiteSpace(partnerCode) ||
                string.IsNullOrWhiteSpace(environment))
            {
                Console.Error.WriteLine("Thiếu --partner hoặc --environment.");
                return 1;
            }

            if (environment is not ("uat" or "prod"))
            {
                Console.Error.WriteLine("--environment chỉ nhận 'uat' hoặc 'prod'.");
                return 1;
            }

            var scopes = string.IsNullOrWhiteSpace(scopesValue) ||
                scopesValue.Equals("all", StringComparison.OrdinalIgnoreCase)
                ? PartnerScopes.All.OrderBy(item => item).ToArray()
                : scopesValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item)
                    .ToArray();

            var invalidScopes = scopes.Where(item => !PartnerScopes.All.Contains(item)).ToArray();
            if (scopes.Length == 0 || invalidScopes.Length > 0)
            {
                Console.Error.WriteLine(
                    invalidScopes.Length > 0
                        ? $"Scope không hợp lệ: {string.Join(", ", invalidScopes)}"
                        : "Phải cấp ít nhất một scope.");
                return 1;
            }

            DateTime? expiresAt = null;
            if (!string.IsNullOrWhiteSpace(expiresValue))
            {
                if (!DateTime.TryParseExact(
                    expiresValue,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var expiresDate))
                {
                    Console.Error.WriteLine("--expires phải theo định dạng yyyy-MM-dd.");
                    return 1;
                }

                expiresAt = expiresDate.Date.AddDays(1).AddTicks(-1);
            }

            var partner = await dbContext.Partners
                .SingleOrDefaultAsync(item => item.Code == partnerCode);
            if (partner == null)
            {
                Console.Error.WriteLine($"Không tìm thấy Partner '{partnerCode}'.");
                return 1;
            }

            if (partner.Status != PartnerStatus.ACTIVE)
            {
                Console.Error.WriteLine($"Partner '{partnerCode}' đang bị vô hiệu hóa.");
                return 1;
            }

            string accessKey;
            do
            {
                accessKey = $"s3_{environment}_{PartnerCredentialSecret.Generate(16)}";
            }
            while (await dbContext.PartnerCredentials.AnyAsync(item => item.AccessKey == accessKey));

            var secretKey = PartnerCredentialSecret.Generate();
            var credential = new PartnerCredential
            {
                PartnerID = partner.ID,
                AccessKey = accessKey,
                SecretKeyHash = PartnerCredentialSecret.Hash(secretKey),
                SecretKey = secretKey,
                Scopes = string.Join(',', scopes),
                Status = PartnerCredentialStatus.ACTIVE,
                ExpiresAt = expiresAt
            };

            dbContext.PartnerCredentials.Add(credential);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("Đã tạo credential. Secret key chỉ được hiển thị lần này.");
            Console.WriteLine($"Partner: {partner.Code}");
            Console.WriteLine($"Access key: {accessKey}");
            Console.WriteLine($"Secret key: {secretKey}");
            Console.WriteLine($"Scopes: {credential.Scopes}");
            Console.WriteLine($"Expires at: {(expiresAt.HasValue ? expiresAt.Value.ToString("yyyy-MM-dd") : "Không hết hạn")}");
            return 0;
        }

        private static bool Matches(string[] args, string resource, string action)
        {
            return args.Length >= 2 &&
                args[0].Equals(resource, StringComparison.OrdinalIgnoreCase) &&
                args[1].Equals(action, StringComparison.OrdinalIgnoreCase);
        }

        private static string? GetOption(string[] args, string name)
        {
            for (var index = 2; index < args.Length - 1; index++)
            {
                if (args[index].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return args[index + 1];
            }

            return null;
        }

        private static void WriteUsage()
        {
            Console.WriteLine("Cách dùng:");
            Console.WriteLine("  partner create --code <CODE> --name <NAME>");
            Console.WriteLine("  credential create --partner <CODE> --environment <uat|prod> [--scopes <scope1,scope2|all>] [--expires yyyy-MM-dd]");
        }
    }
}
