using MediatR;
using Newtonsoft.Json;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Napas;
namespace S3.Gateway.Features.Payments.Napas
{
    public class GetDeepLinkRequest : NpGetDeepLinkRequest, IRequest<ResponseBase<NpGetDeepLinkResponse>>
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }

    public class GetDeepLinkRequestHandler : IRequestHandler<GetDeepLinkRequest, ResponseBase<NpGetDeepLinkResponse>>
    {
        private readonly NapasClient _napasClient;
        private readonly DBContext _dbContext;

        public GetDeepLinkRequestHandler(NapasClient napasClient, DBContext dbContext)
        {
            _napasClient = napasClient;
            _dbContext = dbContext;
        }

        public async Task<ResponseBase<NpGetDeepLinkResponse>> Handle(GetDeepLinkRequest request, CancellationToken cancellationToken)
        {
            var response = new ResponseBase<NpGetDeepLinkResponse>();

            try
            {
                NpGetDeepLinkRequest npGetDeepLinkRequest = request;
                var data = await _napasClient.GetDeepLink(npGetDeepLinkRequest);
                var callbackRouting = new CallbackRouting
                {
                    RequestID = RequestContext.RequestID,
                    Target = Target3rd.Napas,
                    PartnerCode = request.PartnerCode,
                    CallbackUrl = request.CallbackUrl,
                    RequestPayload = JsonConvert.SerializeObject(request),
                    ActionHistory = "GET DEEP LINK",
                };

                var callbackRoutingLog = new CallbackRoutingLog
                {
                    RequestID = callbackRouting.RequestID,
                    Action = callbackRouting.ActionHistory,
                    RequestPayload = JsonConvert.SerializeObject(npGetDeepLinkRequest),
                    ResponsePayload = JsonConvert.SerializeObject(data)
                };

                _dbContext.CallbackRoutings.Add(callbackRouting);
                _dbContext.CallbackRoutingLogs.Add(callbackRoutingLog);
                await _dbContext.SaveChangesAsync(cancellationToken);
                response.IsSuccess = true;
                return response;
            }
            catch
            {
                response.Message = "Có lỗi xảy ra";
                return response;
            }
        }
    }
}