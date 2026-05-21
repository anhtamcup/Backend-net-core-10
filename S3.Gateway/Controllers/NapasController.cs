using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Features.Ekyc.Napas;
using S3.Gateway.Integrations.Ekyc;

namespace S3.Gateway.Controllers
{
    [ApiController]
    [Route("napas")]
    public class NapasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly EkycConfig _eConfig;
        public NapasController(IMediator mediator, IOptions<EkycConfig> options)
        {
            _mediator = mediator;
            _eConfig = options.Value;
        }

        //[HttpPost("callbackstatus")]
        //public async Task<IActionResult> CallBackStatus([FromBody] CallBackStatusRequest request)
        //{
        //    if (_eConfig.Napas.SkipVerifySignature == false)
        //    {
        //        if (!Request.Headers.TryGetValue("Signature", out var signatureBase64))
        //        {
        //            return BadRequest("Missing signature header");
        //        }

        //        var publicKeyPath = Path.Combine(AppContext.BaseDirectory, _eConfig.Napas.PublicKey);
        //        var payload = Utility.SerializeObjectLowerCase(request);
        //        var verifySignature = RSASignatureService.VerifySignature(payload, signatureBase64.ToString(), publicKeyPath);

        //        if (verifySignature == false)
        //        {
        //            return BadRequest("Verify Signature Failed");
        //        }
        //    }

        //    var result = await _mediator.Send(request);
        //    return Ok(result);
        //}

        [HttpPost("callbackstatus")]
        public async Task<IActionResult> CallBackStatus([FromBody] CallBackStatusRequest request)
        {
            if (_eConfig.Napas.SkipVerifySignature == false)
            {
                if (!Request.Headers.TryGetValue("Signature", out var signatureBase64))
                    return BadRequest("Missing signature header");

                // Lấy raw body thay vì serialize lại
                Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body, leaveOpen: true);
                var payload = await reader.ReadToEndAsync();

                var publicKeyPath = Path.Combine(AppContext.BaseDirectory, _eConfig.Napas.PublicKey);
                var verifySignature = RSASignatureService.VerifySignature(payload, signatureBase64.ToString(), publicKeyPath);

                if (verifySignature == false)
                    return BadRequest("Verify Signature Failed");
            }

            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
