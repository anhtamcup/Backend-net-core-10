using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Integrations.Payment;
using S3.Gateway.Integrations.Payment.Napas;
using System.Text;

namespace S3.Gateway.Features.Payments.Napas
{
    public class PaymentNotificationRequest : NpRequestBase<NpPaymentNotificationRequest>, IRequest<NpPaymentNotificationResponse>
    {
    }

    public class PaymentNotificationRequestHandler : IRequestHandler<PaymentNotificationRequest, NpPaymentNotificationResponse>
    {
        private readonly DBContext _dbContext;
        private readonly PaymentConfig _pConfig;

        public PaymentNotificationRequestHandler(DBContext dbContext, IOptions<PaymentConfig> options)
        {
            _dbContext = dbContext;
            _pConfig = options.Value;
        }

        public async Task<NpPaymentNotificationResponse> Handle(PaymentNotificationRequest request, CancellationToken cancellationToken)
        {

            var payload = Utility.SerializeObjectLowerCase(request);
            using (var client = new HttpClient())
            using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
            {
                var forwardResponse = await client.PostAsync(_pConfig.Napas.ForwardNotiPaymentURL, content);
                var resultForwardString = await forwardResponse.Content.ReadAsStringAsync();
                var resultForward = JsonConvert.DeserializeObject<NpPaymentNotificationResponse>(resultForwardString);
                return resultForward;
            }
        }
    }
}