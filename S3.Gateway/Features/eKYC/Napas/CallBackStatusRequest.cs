using MediatR;
using Newtonsoft.Json;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Ekyc.Napas;
using S3.Gateway.Integrations.Payment.Napas;
using System.Text;

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
            try
            {
                var now = DateTime.Now;
                var callbackRoutingLog = new CallbackRoutingLog
                {
                    RequestID = RequestContext.RequestID,
                    Action = "RECEIVED NOTI FROM NAPAS",
                    RequestPayload = Utility.SerializeObjectLowerCase(request),
                };

                _dbContext.CallbackRoutingLogs.Add(callbackRoutingLog);

                // Forward to merchant system
                var callbackRouting = _dbContext.CallbackRoutings.Where(item => item.RefID == request.PlatformMerchantId).FirstOrDefault();
                if (callbackRouting == null)
                {
                    callbackRoutingLog.ErrorMessage = "Không tìm thấy thông tin call back routing";
                    throw new Exception(callbackRoutingLog.ErrorMessage);
                }

                callbackRouting.ActionHistory += string.Format(" -> {0} : {1}", callbackRoutingLog.Action, callbackRoutingLog.RequestID);
                callbackRouting.UpdatedAt = now;

                // Forward
                var endpointForward = callbackRouting.CallbackUrl;

                try
                {

                    var forwardResponse = await _apiClient.PostTAsync<NpCallBackStatusResponse>(endpointForward, callbackRoutingLog.RequestPayload);
                    var forwardSucess = (forwardResponse != null);

                    //using (var client = new HttpClient())
                    //using (var content = new StringContent(callbackRoutingLog.RequestPayload, Encoding.UTF8, "application/json"))
                    //{
                    //    var forwardResponse = await client.PostAsync(endpointForward, content);
                    //    var resultForwardString = await forwardResponse.Content.ReadAsStringAsync();
                    //    var resultForward = JsonConvert.DeserializeObject<NpCallBackStatusResponse>(resultForwardString);
                    //var callbackRoutingLog2 = new CallbackRoutingLog
                    //{
                    //    RequestID = RequestContext.RequestID,
                    //    Action = "FORWARD TO: " + endpointForward,
                    //    RequestPayload = callbackRoutingLog.ResponsePayload,
                    //    ResponsePayload = resultForwardString,
                    //    IsSuccess = (forwardResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    //};

                    callbackRoutingLog.IsSuccess = forwardSucess;
                    //callbackRouting.ActionHistory += string.Format(" -> {0} : {1}", callbackRoutingLog2.Action, callbackRoutingLog2.RequestID);
                    return forwardResponse;
                    //}
                }
                catch (Exception ex)
                {
                    callbackRouting.ActionHistory += "   -> FORWARD FAILD TO: " + endpointForward;
                    throw new Exception(ex.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}