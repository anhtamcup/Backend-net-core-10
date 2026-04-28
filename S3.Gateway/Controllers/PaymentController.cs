using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Features.Payments.Napas;
using S3.Gateway.Integrations.Payment;

namespace S3.Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            var publicKeyPath = Path.Combine(AppContext.BaseDirectory, _pConfig.Napas.PublicKey);
            var payloadVerify = Utility.SerializeObjectLowerCase(request.Payload);
            var verifySignature = RSASignatureService.VerifySignature(payloadVerify, request.Header.Signature, publicKeyPath);
            //if(verifySignature == false)
            //{
            //    return BadRequest("Verify Signature Failed");
            //}

            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("napas/qr")]
        public async Task<IActionResult> NapasGenQR([FromBody] NapasGenQRRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("napas/investigation")]
        public async Task<IActionResult> NapasInvestigation([FromBody] NapasInvestigationRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
