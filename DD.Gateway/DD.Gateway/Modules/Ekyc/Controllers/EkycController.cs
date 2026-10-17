using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DD.Gateway.Data;
using DD.Gateway.Entities;
using DD.Gateway.Modules.Ekyc.NamABank;
using DD.Gateway.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DD.Gateway.Modules.Ekyc.Controllers;

[ApiController]
[Route("api/ekyc")]
public sealed class EkycController(
    NamABankDeepLinkService deepLinkService,
    NamABankCallbackVerificationService callbackVerificationService,
    GatewayDbContext dbContext,
    ILogger<EkycController> logger) : ControllerBase
{
    [HttpPost("deeplinks")]
    [Authorize(Policy = PartnerScopes.EkycDeepLinkCreate)]
    [ProducesResponseType<NamABankDeepLinkResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<NamABankDeepLinkResult>> CreateDeepLink(
        [FromBody] CreateDeepLinkRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var partnerId = AuthenticatedPartnerId();
            var result = deepLinkService.Create(request.PartnerId, request.CallbackUrl);
            dbContext.EkycSessions.Add(new EkycSession
            {
                SessionId = result.SessionId,
                PartnerId = partnerId,
                NabPartnerId = request.PartnerId,
                CallbackUrl = request.CallbackUrl,
                IssuedAt = result.IssuedAt,
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            return Ok(result);
        }
        catch (NamABankEkycRequestException error)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Dữ liệu tạo link eKYC không hợp lệ.", detail: error.Message);
        }
        catch (NamABankEkycConfigurationException error)
        {
            logger.LogWarning(error, "NamABank eKYC deeplink configuration is incomplete or invalid.");
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Chưa thể tạo link eKYC NamABank.", detail: error.Message);
        }
    }

    [HttpPost("callback/verify")]
    [Authorize(Policy = PartnerScopes.EkycDeepLinkCreate)]
    [ProducesResponseType<VerifiedCallbackResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<VerifiedCallbackResult>> VerifyCallback(
        [FromBody] VerifyCallbackRequest request, CancellationToken cancellationToken)
    {
        VerifiedCallbackResult result;
        try
        {
            result = callbackVerificationService.Verify(request.Result);
        }
        catch (NamABankEkycRequestException error)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Kết quả callback NAB không hợp lệ.", detail: error.Message);
        }
        catch (NamABankEkycConfigurationException error)
        {
            logger.LogWarning(error, "NamABank callback verification key is unavailable.");
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Chưa thể xác minh callback NAB.", detail: error.Message);
        }

        var partnerId = AuthenticatedPartnerId();
        var resultHash = Convert.ToHexString(SHA256.HashData(Encoding.ASCII.GetBytes(request.Result)));
        var updated = await dbContext.EkycSessions
            .Where(session => session.SessionId == result.SessionId
                && session.PartnerId == partnerId && session.ResultHash == null)
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
                && item.PartnerId == partnerId, cancellationToken);
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
