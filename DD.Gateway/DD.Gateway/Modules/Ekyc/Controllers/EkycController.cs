using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DD.Gateway.Data;
using DD.Gateway.Entities;
using DD.Gateway.Modules.Ekyc.Contracts;
using DD.Gateway.Modules.Ekyc.Providers;
using DD.Gateway.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DD.Gateway.Modules.Ekyc.Controllers;

[ApiController]
[Route("api/ekyc")]
public sealed class EkycController(
    EkycProviderRegistry providerRegistry,
    GatewayDbContext dbContext,
    ILogger<EkycController> logger) : ControllerBase
{
    [HttpPost("deeplinks")]
    [Authorize(Policy = PartnerScopes.EkycDeepLinkCreate)]
    [ProducesResponseType<EkycDeepLinkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EkycDeepLinkResult>> CreateDeepLink(
        [FromBody] CreateDeepLinkRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var partnerId = AuthenticatedPartnerId();
            var provider = providerRegistry.GetRequired(request.Provider);
            var result = provider.CreateDeepLink(request.PartnerId, request.CallbackUrl);
            dbContext.EkycSessions.Add(new EkycSession
            {
                SessionId = result.SessionId,
                PartnerId = partnerId,
                Provider = result.Provider,
                ProviderPartnerId = request.PartnerId,
                CallbackUrl = request.CallbackUrl,
                IssuedAt = result.IssuedAt,
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(result);
        }
        catch (UnsupportedEkycProviderException error)
        {
            return BadRequest(new { message = error.Message });
        }
        catch (EkycProviderRequestException error)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Dữ liệu tạo link eKYC không hợp lệ.", detail: error.Message);
        }
        catch (EkycProviderConfigurationException error)
        {
            logger.LogWarning(error, "eKYC provider {Provider} configuration is incomplete or invalid.", request.Provider);
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Chưa thể tạo link eKYC.", detail: error.Message);
        }
    }

    [HttpPost("callback/verify")]
    [Authorize(Policy = PartnerScopes.EkycDeepLinkCreate)]
    [ProducesResponseType<EkycVerifiedResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<EkycVerifiedResult>> VerifyCallback(
        [FromBody] VerifyCallbackRequest request, CancellationToken cancellationToken)
    {
        EkycVerifiedResult result;
        IEkycProvider provider;
        try
        {
            provider = providerRegistry.GetRequired(request.Provider);
            result = provider.VerifyCallback(request.Result);
        }
        catch (UnsupportedEkycProviderException error)
        {
            return BadRequest(new { message = error.Message });
        }
        catch (EkycProviderRequestException error)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Kết quả callback không hợp lệ.", detail: error.Message);
        }
        catch (EkycProviderConfigurationException error)
        {
            logger.LogWarning(error, "eKYC provider {Provider} callback verification is unavailable.", request.Provider);
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Chưa thể xác minh callback eKYC.", detail: error.Message);
        }

        var partnerId = AuthenticatedPartnerId();
        var resultHash = Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(request.Result)));
        var updated = await dbContext.EkycSessions
            .Where(session => session.SessionId == result.SessionId
                && session.PartnerId == partnerId
                && session.Provider == provider.Name
                && session.ResultHash == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(session => session.ResultHash, resultHash)
                .SetProperty(session => session.VerifiedAt, DateTime.UtcNow)
                .SetProperty(session => session.Status, result.Status)
                .SetProperty(session => session.Code, result.Code), cancellationToken);

        if (updated == 1)
        {
            return Ok(result);
        }

        var session = await dbContext.EkycSessions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.SessionId == result.SessionId
                && item.PartnerId == partnerId
                && item.Provider == provider.Name, cancellationToken);
        if (session is null)
        {
            return NotFound(new { Message = "Không tìm thấy phiên eKYC của đối tác." });
        }

        if (session.ResultHash != resultHash)
        {
            return Conflict(new { Message = "Phiên eKYC đã có kết quả khác." });
        }

        return Ok(result);
    }

    private int AuthenticatedPartnerId() =>
        int.Parse(User.FindFirstValue(PartnerAccessKeyDefaults.PartnerIdClaim)!);
}
