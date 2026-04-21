using MediatR;
using S3.Gateway.Integrations.Napas;

namespace S3.Gateway.Features.Payments.Napas
{
    public class CallBackStatusRequest : NpCallBackStatusRequest, IRequest<NpCallBackStatusResponse>
    {

    }

    public class CallBackStatusRequestHandler : IRequestHandler<CallBackStatusRequest, NpCallBackStatusResponse>
    {
        private readonly NapasClient _napasClient;

        public CallBackStatusRequestHandler(NapasClient napasClient)
        {
            _napasClient = napasClient;
        }

        public async Task<NpCallBackStatusResponse> Handle(CallBackStatusRequest request, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            // Forward to merchant system
            var response = new NpCallBackStatusResponse();

            return response;
        }
    }
}