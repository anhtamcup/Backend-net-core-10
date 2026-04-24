using MediatR;
using S3.Gateway.Data;
using S3.Gateway.Integrations.eKYC.Napas;

namespace S3.Gateway.Features.Payment.Napas
{
    public class PaymentNotificationRequest : NpRequestBase<NpPaymentNotificationRequest>, IRequest<NpPaymentNotificationResponse>
    {
    }

    public class PaymentNotificationRequestHandler : IRequestHandler<PaymentNotificationRequest, NpPaymentNotificationResponse>
    {
        private readonly NapasClient _napasClient;
        private readonly DBContext _dbContext;

        public PaymentNotificationRequestHandler(NapasClient napasClient, DBContext dbContext)
        {
            _napasClient = napasClient;
            _dbContext = dbContext;
        }

        public async Task<NpPaymentNotificationResponse> Handle(PaymentNotificationRequest request, CancellationToken cancellationToken)
        {
            return new NpPaymentNotificationResponse();
        }
    }
}