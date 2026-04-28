using MediatR;
using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Payment;
using S3.Gateway.Integrations.Payment.Napas;
using static S3.Gateway.Features.Payments.Napas.TransIdCodecHelper;

namespace S3.Gateway.Features.Payments.Napas
{
    public class PaymentNotificationRequest : NpRequestBase<NpPaymentNotificationRequest>, IRequest<NpPaymentNotificationResponse>
    {
    }

    public class PaymentNotificationRequestHandler : IRequestHandler<PaymentNotificationRequest, NpPaymentNotificationResponse>
    {
        private readonly DBContext _dbContext;
        private readonly PaymentConfig _pConfig;
        private readonly BaseApiClient _apiClient;

        public PaymentNotificationRequestHandler(DBContext dbContext, IOptions<PaymentConfig> options, BaseApiClient apiClient)
        {
            _dbContext = dbContext;
            _pConfig = options.Value;
            _apiClient = apiClient;
        }

        public async Task<NpPaymentNotificationResponse> Handle(PaymentNotificationRequest request, CancellationToken cancellationToken)
        {
            var billNumberEncode = request.Payload.Id[^7..];
            var decodeData = TransIdCodecHelper.Decode(billNumberEncode);
            var payload = Utility.SerializeObjectLowerCase(request);

            switch (decodeData.Type.GetNapasType())
            {
                case NapasTransactionInvoiceType.Forward:
                    var billNumber = decodeData.Id;
                    var routing = _dbContext.CallbackRoutings.Where(item => item.ID == billNumber).FirstOrDefault();
                    if (routing == null) throw new Exception("Bill number không hợp lệ");

                    var forwardUrl = routing.CallbackUrl;
                    var forwardResponse = await _apiClient.PostTAsync<NpPaymentNotificationResponse>(forwardUrl, payload);
                    var forwardSucess = (forwardResponse != null);
                    var routingLog = new CallbackRoutingLog
                    {
                        Action = forwardSucess ? "FORWARD COMPLETED" : "FORWARD ERROR",
                        RequestPayload = payload,
                        ResponsePayload = forwardSucess
                            ? Utility.SerializeObjectLowerCase(forwardResponse)
                            : string.Empty,
                        IsSuccess = forwardSucess,
                    };

                    routing.ActionHistory += "  -> " + routingLog.Action;
                    routing.Status = routingLog.IsSuccess ? CallbackRoutingStatus.COMPLETED : CallbackRoutingStatus.ERROR;

                    _dbContext.CallbackRoutingLogs.Add(routingLog);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return forwardResponse;
                default:
                    var s3Response = await _apiClient.PostTAsync<NpPaymentNotificationResponse>(_pConfig.Napas.ForwardNotiPaymentURL, payload);
                    return s3Response;
            }
        }
    }
}