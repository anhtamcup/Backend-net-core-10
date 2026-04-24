using MediatR;
using Microsoft.AspNetCore.Mvc;
using S3.Gateway.Features.eKYC.Napas;

namespace S3.Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NapasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public NapasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("merchant/getdeeplink")]
        public async Task<IActionResult> Getdeeplink([FromBody] GetDeepLinkRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("callbackstatus")]
        public async Task<IActionResult> CallBackStatus([FromBody] CallBackStatusRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("merchant/createmerchant")]
        public async Task<IActionResult> CreateMerchant([FromBody] CreateMerchantRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("merchant/updatebankaccount")]
        public async Task<IActionResult> UpdateBankAccount([FromBody] UpdateBankAccountRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("merchant/querymerchant")]
        public async Task<IActionResult> QueryMerchant([FromBody] QueryMerchantRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("merchant/updatemerchant")]
        public async Task<IActionResult> UpdateMerchant([FromBody] UpdateMerchantRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("oath")]
        public async Task<IActionResult> OAuth([FromBody] OAuthRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
