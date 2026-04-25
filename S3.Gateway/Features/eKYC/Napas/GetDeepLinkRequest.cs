using Azure.Core;
using MediatR;
using Newtonsoft.Json;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.eKYC.Napas;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace S3.Gateway.Features.eKYC.Napas
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
                var npGetDeepLinkRequest = new NpGetDeepLinkRequest
                {
                    SenderReference = request.SenderReference,
                    CreationDateTime = request.CreationDateTime,
                    PlatformCode = request.PlatformCode,
                    PlatformName = request.PlatformName,
                    PlatformMerchantId = request.PlatformMerchantId,
                    MobileOS = request.MobileOS,
                };
                    
                var data = await _napasClient.GetDeepLink(npGetDeepLinkRequest);

                var callbackRouting = new CallbackRouting
                {
                    RefID = request.PlatformMerchantId,
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
                response.Data = data;
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