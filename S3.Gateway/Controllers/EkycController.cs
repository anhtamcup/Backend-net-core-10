using MediatR;
using Microsoft.AspNetCore.Mvc;
using S3.Gateway.Features.eKYC.Napas;

namespace S3.Gateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EkycController : ControllerBase
    {
        private readonly IMediator _mediator;
        public EkycController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("napas/merchant/getdeeplink")]
        public async Task<IActionResult> Getdeeplink([FromBody] GetDeepLinkRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("napas/callbackstatus")]
        public async Task<IActionResult> CallBackStatus([FromBody] CallBackStatusRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("napas/merchant/createmerchant")]
        public async Task<IActionResult> CreateMerchant([FromBody] CreateMerchantRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("napas/merchant/updatebankaccount")]
        public async Task<IActionResult> UpdateBankAccount([FromBody] UpdateBankAccountRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("napas/merchant/querymerchant")]
        public async Task<IActionResult> QueryMerchant([FromBody] QueryMerchantRequest request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("napas/merchant/updatemerchant")]
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
