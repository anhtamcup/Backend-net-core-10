using MediatR;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Ekyc.Napas;
using S3.Gateway.Security;

namespace S3.Gateway.Features.Ekyc.Napas
{
    public class GetDeepLinkRequest : NpGetDeepLinkRequest, IRequest<ResponseBase<NpGetDeepLinkResponse>>
    {
        public string CallbackUrl { get; set; } = string.Empty;
    }

    public class GetDeepLinkRequestHandler : IRequestHandler<GetDeepLinkRequest, ResponseBase<NpGetDeepLinkResponse>>
    {
        private readonly NapasClient _napasClient;
        private readonly DBContext _dbContext;
        private readonly IPartnerContext _partnerContext;

        public GetDeepLinkRequestHandler(
            NapasClient napasClient,
            DBContext dbContext,
            IPartnerContext partnerContext)
        {
            _napasClient = napasClient;
            _dbContext = dbContext;
            _partnerContext = partnerContext;
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

                if (data.Response.Code == ApiErrorCode.SUCCESS.Code)
                {
                    var callbackRouting = new CallbackRouting
                    {
                        PartnerID = _partnerContext.PartnerID,
                        PartnerCredentialID = _partnerContext.CredentialID,
                        RefID = request.PlatformMerchantId,
                        RequestID = RequestContext.RequestID,
                        Target = Target3rd.Napas,
                        PartnerCode = _partnerContext.PartnerCode,
                        CallbackUrl = request.CallbackUrl,
                        RequestPayload = Utility.SerializeObjectLowerCase(request),
                        ActionHistory = "GET DEEP LINK",
                    };

                    var callbackRoutingLog = new CallbackRoutingLog
                    {
                        RequestID = callbackRouting.RequestID,
                        Action = callbackRouting.ActionHistory,
                        RequestPayload = Utility.SerializeObjectLowerCase(npGetDeepLinkRequest),
                        ResponsePayload = Utility.SerializeObjectLowerCase(data),
                        IsSuccess = true
                    };

                    _dbContext.CallbackRoutings.Add(callbackRouting);
                    _dbContext.CallbackRoutingLogs.Add(callbackRoutingLog);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

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
