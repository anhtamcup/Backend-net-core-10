using MediatR;
using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Payment;
using S3.Gateway.Integrations.Payment.Napas;

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
            var payload = Utility.SerializeObjectLowerCase(request);
            var response = await _apiClient.PostTAsync<NpPaymentNotificationResponse>(_pConfig.Napas.ForwardNotiPaymentURL, payload);
            return response;
        }
    }
}