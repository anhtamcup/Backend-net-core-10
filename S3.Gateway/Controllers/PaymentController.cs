using MediatR;
using Microsoft.AspNetCore.Mvc;
using S3.Gateway.Features.Payment.Napas;

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
    }
}
