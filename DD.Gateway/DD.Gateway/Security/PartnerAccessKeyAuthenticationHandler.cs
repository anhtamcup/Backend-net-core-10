using System.Security.Claims;
using System.Text.Encodings.Web;
using DD.Gateway.Data;
using DD.Gateway.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DD.Gateway.Security;

public sealed class PartnerAccessKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    GatewayDbContext dbContext)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var hasAccessKey = Request.Headers.TryGetValue(
            PartnerAccessKeyDefaults.AccessKeyHeader, out var accessKeyValues);
        var hasSecretKey = Request.Headers.TryGetValue(
            PartnerAccessKeyDefaults.SecretKeyHeader, out var secretKeyValues);

        if (!hasAccessKey && !hasSecretKey)
        {
            return AuthenticateResult.NoResult();
        }

        if (!hasAccessKey || !hasSecretKey || accessKeyValues.Count != 1 || secretKeyValues.Count != 1)
        {
            return AuthenticateResult.Fail("Thông tin xác thực không hợp lệ");
        }

        var accessKey = accessKeyValues[0]?.Trim();
        var secretKey = secretKeyValues[0];
        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            return AuthenticateResult.Fail("Thông tin xác thực không hợp lệ");
        }

        var credential = await dbContext.PartnerCredentials
            .Include(item => item.Partner)
            .SingleOrDefaultAsync(item => item.AccessKey == accessKey, Context.RequestAborted);

        var now = DateTime.UtcNow;
        if (credential is null
            || credential.Status != PartnerCredentialStatus.Active
            || credential.Partner.Status != PartnerStatus.Active
            || credential.ExpiresAt is { } expiresAt && expiresAt <= now
            || !PartnerCredentialSecret.Verify(secretKey, credential.SecretKeyHash))
        {
            return AuthenticateResult.Fail("Thông tin xác thực không hợp lệ");
        }

        var claims = new List<Claim>
        {
            new(PartnerAccessKeyDefaults.PartnerIdClaim, credential.PartnerId.ToString()),
            new(PartnerAccessKeyDefaults.PartnerCodeClaim, credential.Partner.Code),
            new(PartnerAccessKeyDefaults.CredentialIdClaim, credential.Id.ToString())
        };

        foreach (var scope in credential.Scopes.Split(
                     ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            claims.Add(new Claim(PartnerAccessKeyDefaults.ScopeClaim, scope));
        }

        if (credential.LastUsedAt is null || credential.LastUsedAt < now.AddMinutes(-5))
        {
            credential.LastUsedAt = now;
            credential.UpdatedAt = now;
            await dbContext.SaveChangesAsync(Context.RequestAborted);
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }
}
