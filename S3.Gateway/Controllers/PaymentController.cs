using MediatR;
using Microsoft.AspNetCore.Mvc;
using S3.Gateway.Features.Payment.Napas;
using S3.Gateway.Features.Payments.Napas;

namespace S3.Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("napas/apg/notification")]
        public async Task<IActionResult> NapasNotification([FromBody] PaymentNotificationRequest request)
        {
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
        public async Task<IActionResult> NapasInvestigation([FromBody] NapasGenQRRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
