using MediatR;
using Microsoft.Extensions.Options;
using S3.Gateway.Data;
using S3.Gateway.Integrations.Payment;
using S3.Gateway.Integrations.Payment.Napas;

namespace S3.Gateway.Features.Payments.Napas
{
    public class NapasInvestigationRequest: IRequest<ResponseBase<NpRequestBase<NpInvestigationResponse>>>
    {
        public string ID { get; set; } = string.Empty;
        public string TransDateTime { get; set; } = string.Empty;
    }

    public class InvestigationHandler : IRequestHandler<NapasInvestigationRequest, ResponseBase<NpRequestBase<NpInvestigationResponse>>>
    {
        private readonly DBContext _dbContext;
        private readonly PaymentConfig _pConfig;
        private readonly NapasApi _napasAPI;

        public InvestigationHandler(DBContext dbContext, IOptions<PaymentConfig> options, NapasApi napasAPI)
        {
            _dbContext = dbContext;
            _pConfig = options.Value;
            _napasAPI = napasAPI;
        }

        public async Task<ResponseBase<NpRequestBase<NpInvestigationResponse>>> Handle(NapasInvestigationRequest request, CancellationToken cancellationToken)
        {
            var napasRequest = new NpInvestigationRequest
            {
                Id = request.ID,
                TransDateTime = request.TransDateTime
            };

            var response = await _napasAPI.Investigation(napasRequest);
            var result = new ResponseBase<NpRequestBase<NpInvestigationResponse>>();
            result.IsSuccess = true;
            result.Data = response;
            return result;
        }
    }
}
