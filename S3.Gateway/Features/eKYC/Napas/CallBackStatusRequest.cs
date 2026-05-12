using MediatR;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Ekyc.Napas;

namespace S3.Gateway.Features.Ekyc.Napas
{
    public class CallBackStatusRequest : NpCallBackStatusRequest, IRequest<NpCallBackStatusResponse>
    {

    }

    public class CallBackStatusRequestHandler : IRequestHandler<CallBackStatusRequest, NpCallBackStatusResponse>
    {
        private readonly DBContext _dbContext;
        private readonly BaseApiClient _apiClient;

        public CallBackStatusRequestHandler(DBContext dbContext, BaseApiClient apiClient)
        {
            _dbContext = dbContext;
            _apiClient = apiClient;
        }

        public async Task<NpCallBackStatusResponse> Handle(CallBackStatusRequest request, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var requestPayloadReceive = Utility.SerializeObjectLowerCase(request);
            var errorMessage = string.Empty;
            var refID = string.Empty;
            var response = new NpCallBackStatusResponse
            {
                SenderReference = request.SenderReference,
                CreationDateTime = request.CreationDateTime,
                Response = new NpResponseInfo
                {
                    Code = "500",
                    Description = "Có lỗi xảy ra"
                }
            };

            try
            {
                var callbackRouting = _dbContext.CallbackRoutings
                    .Where(item => item.RefID == request.PlatformMerchantId)
                    .OrderByDescending(item => item.ID)
                    .FirstOrDefault();

                if (callbackRouting == null)
                {
                    errorMessage = "Không tìm thấy thông tin call back routing";
                    response.Response.Details = errorMessage;
                    return response;
                }

                refID = callbackRouting.ID.ToString();
                var url = callbackRouting.CallbackUrl;
                var forwardResponse = await _apiClient.PostTAsync<NpCallBackStatusResponse>(url, requestPayloadReceive);
                if (forwardResponse == null)
                {
                    response.Response.Details = "Lỗi chuyển tiếp";
                    callbackRouting.ActionHistory += " -> Forward thất bại đến: " + url;
                    callbackRouting.Status = CallbackRoutingStatus.ERROR;
                    return response;
                }

                var callbackRoutingLogForward = new CallbackRoutingLog
                {
                    RefID = refID,
                    Action = "FORWARD",
                    RequestPayload = requestPayloadReceive,
                    ResponsePayload = Utility.SerializeObjectLowerCase(forwardResponse),
                    ErrorMessage = forwardResponse.Response.Details
                };
                _dbContext.CallbackRoutingLogs.Add(callbackRoutingLogForward);

                callbackRouting.ActionHistory += " -> Đã forward đến: " + url;
                callbackRouting.Status = CallbackRoutingStatus.COMPLETED;
                response = forwardResponse;
                return forwardResponse;
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
                response.Response.Details = "Lỗi hệ thống";
                return response;
            }
            finally
            {
                var callbackRoutingLog = new CallbackRoutingLog
                {
                    RefID = refID,
                    Action = "RECEIVED NAPAS",
                    RequestPayload = requestPayloadReceive,
                    ResponsePayload = Utility.SerializeObjectLowerCase(response),
                    ErrorMessage = errorMessage
                };

                _dbContext.CallbackRoutingLogs.Add(callbackRoutingLog);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}