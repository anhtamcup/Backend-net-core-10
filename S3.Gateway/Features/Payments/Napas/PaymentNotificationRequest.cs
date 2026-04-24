using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using S3.Gateway.Data;
using S3.Gateway.Integrations.eKYC.Napas;
using System.Text;

namespace S3.Gateway.Features.Payment.Napas
{
    public class PaymentNotificationRequest : NpRequestBase<NpPaymentNotificationRequest>, IRequest<NpPaymentNotificationResponse>
    {
    }

    public class PaymentNotificationRequestHandler : IRequestHandler<PaymentNotificationRequest, NpPaymentNotificationResponse>
    {
        private readonly NapasClient _napasClient;
        private readonly DBContext _dbContext;
        private readonly NapasConfig _napasConfig;

        public PaymentNotificationRequestHandler(NapasClient napasClient, DBContext dbContext, IOptions<NapasConfig> options)
        {
            _napasClient = napasClient;
            _dbContext = dbContext;
            _napasConfig = options.Value;
        }

        public async Task<NpPaymentNotificationResponse> Handle(PaymentNotificationRequest request, CancellationToken cancellationToken)
        {
            var payload = JsonConvert.SerializeObject(
                request,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });

            using (var client = new HttpClient())
            using (var content = new StringContent(payload, Encoding.UTF8, "application/json"))
            {
                var forwardResponse = await client.PostAsync(_napasConfig.ForwardNotiPaymentURL, content);
                var resultForwardString = await forwardResponse.Content.ReadAsStringAsync();
                var resultForward = JsonConvert.DeserializeObject<NpPaymentNotificationResponse>(resultForwardString);
                return resultForward;
            }
        }
    }
}