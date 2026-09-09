using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using S3.Gateway.Data;
using S3.Gateway.Entities;

namespace S3.Gateway.Security
{
    public class PartnerAccessKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly DBContext _dbContext;

        public PartnerAccessKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            DBContext dbContext)
            : base(options, logger, encoder)
        {
            _dbContext = dbContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var hasAccessKey = Request.Headers.TryGetValue(
                PartnerAccessKeyDefaults.AccessKeyHeader, out var accessKeyValues);
            var hasSecretKey = Request.Headers.TryGetValue(
                PartnerAccessKeyDefaults.SecretKeyHeader, out var secretKeyValues);

            if (!hasAccessKey && !hasSecretKey)
                return AuthenticateResult.NoResult();

            if (!hasAccessKey || !hasSecretKey || accessKeyValues.Count != 1 || secretKeyValues.Count != 1)
                return AuthenticateResult.Fail("Thông tin xác thực không hợp lệ");

            var accessKey = accessKeyValues[0]?.Trim();
            var secretKey = secretKeyValues[0];
            if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrEmpty(secretKey))
                return AuthenticateResult.Fail("Thông tin xác thực không hợp lệ");

            var credential = await _dbContext.PartnerCredentials
                .Include(item => item.Partner)
                .SingleOrDefaultAsync(item => item.AccessKey == accessKey, Context.RequestAborted);

            var now = DateTime.Now;
            if (credential == null ||
                credential.Status != PartnerCredentialStatus.ACTIVE ||
                credential.Partner.Status != PartnerStatus.ACTIVE ||
                credential.ExpiresAt <= now ||
                !PartnerCredentialSecret.Verify(secretKey, credential.SecretKeyHash))
            {
                return AuthenticateResult.Fail("Thông tin xác thực không hợp lệ");
            }

            var claims = new List<Claim>
            {
                new(PartnerAccessKeyDefaults.PartnerIdClaim, credential.PartnerID.ToString()),
                new(PartnerAccessKeyDefaults.PartnerCodeClaim, credential.Partner.Code),
                new(PartnerAccessKeyDefaults.CredentialIdClaim, credential.ID.ToString())
            };

            foreach (var scope in credential.Scopes.Split(
                ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(PartnerAccessKeyDefaults.ScopeClaim, scope));
            }

            if (!credential.LastUsedAt.HasValue || credential.LastUsedAt.Value < now.AddMinutes(-5))
            {
                credential.LastUsedAt = now;
                credential.UpdatedAt = now;
                await _dbContext.SaveChangesAsync(Context.RequestAborted);
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            return AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
        }
    }
}
