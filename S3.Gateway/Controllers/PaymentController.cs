using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Features.Payments.Napas;
using S3.Gateway.Integrations.Payment;
using System.Text;

namespace S3.Gateway.Controllers
{
    [ApiController]
    [Route("payment")]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly PaymentConfig _pConfig;

        public PaymentController(IMediator mediator, IOptions<PaymentConfig> options)
        {
            _mediator = mediator;
            _pConfig = options.Value;
        }

        [HttpPost("napas/apg/notification")]
        public async Task<IActionResult> NapasNotification([FromBody] PaymentNotificationRequest request)
        {
            //Request.Body.Position = 0;
            //using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
            //var rawBody = await reader.ReadToEndAsync();
            //Request.Body.Position = 0;

            var publicKeyPath = Path.Combine(AppContext.BaseDirectory, _pConfig.Napas.PublicKey);
            var payloadVerify = Utility.SerializeObjectLowerCase(request.Payload);
            var verifySignature = RSASignatureService.VerifySignature(payloadVerify, request.Header.Signature, publicKeyPath);
            if (verifySignature == false)
            {
                return BadRequest("Verify Signature Failed");
            }

            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("pmnp/qr")]
        public async Task<IActionResult> NapasGenQR([FromBody] NapasGenQRRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("pmnp/investigation")]
        public async Task<IActionResult> NapasInvestigation([FromBody] NapasInvestigationRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
