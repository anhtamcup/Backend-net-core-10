using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Features.Ekyc.Napas;
using S3.Gateway.Integrations.Ekyc;
using System.Text;

namespace S3.Gateway.Controllers
{
    [ApiController]
    [Route("ekyc")]
    public class EkycController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly EkycConfig _eConfig;
        public EkycController(IMediator mediator, IOptions<EkycConfig> options)
        {
            _mediator = mediator;
            _eConfig = options.Value;
        }

        [HttpPost("pmnp/merchant/getdeeplink")]
        public async Task<IActionResult> Getdeeplink([FromBody] GetDeepLinkRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("pmnp/callbackstatus")]
        public async Task<IActionResult> CallBackStatus([FromBody] CallBackStatusRequest request)
        {
            if (_eConfig.Napas.SkipVerifySignature == false)
            {
                if (!Request.Headers.TryGetValue("Signature", out var signatureBase64))
                {
                    return BadRequest("Missing signature header");
                }

                Request.Body.Position = 0;
                using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
                var rawBody = await reader.ReadToEndAsync();
                Request.Body.Position = 0;

                var publicKeyPath = Path.Combine(AppContext.BaseDirectory, _eConfig.Napas.PublicKey);
                var verifySignature = RSASignatureService.VerifySignature(rawBody, signatureBase64.ToString(), publicKeyPath);

                if (verifySignature == false)
                {
                    return BadRequest("Verify Signature Failed");
                }
            }

            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("pmnp/merchant/createmerchant")]
        public async Task<IActionResult> CreateMerchant([FromBody] CreateMerchantRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("pmnp/merchant/updatebankaccount")]
        public async Task<IActionResult> UpdateBankAccount([FromBody] UpdateBankAccountRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("pmnp/merchant/querymerchant")]
        public async Task<IActionResult> QueryMerchant([FromBody] QueryMerchantRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("pmnp/merchant/updatemerchant")]
        public async Task<IActionResult> UpdateMerchant([FromBody] UpdateMerchantRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        //[HttpPost("napas/oath")]
        //public async Task<IActionResult> OAuth([FromBody] OAuthRequest request)
        //{
        //    var result = await _mediator.Send(request);
        //    return Ok(result);
        //}
    }
}
