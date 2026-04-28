using MediatR;
using Microsoft.Extensions.Options;
using S3.Gateway.Data;
using S3.Gateway.Integrations.Ekyc.Napas;
using S3.Gateway.Integrations.Payment.Napas;

namespace S3.Gateway.Features.Payments.Napas
{
    public class NapasInvestigationRequest : NpInvestigationRequest, IRequest<NpRequestBase<NpInvestigationResponse>>
    {

    }

    public class InvestigationHandler : IRequestHandler<NapasInvestigationRequest, NpRequestBase<NpInvestigationResponse>>
    {
        private readonly DBContext _dbContext;
        private readonly NapasConfig _napasConfig;

        public InvestigationHandler(DBContext dbContext, IOptions<NapasConfig> options)
        {
            _dbContext = dbContext;
            _napasConfig = options.Value;
        }

        public async Task<NpRequestBase<NpInvestigationResponse>> Handle(NapasInvestigationRequest request, CancellationToken cancellationToken)
        {
            var response = new NpRequestBase<NpInvestigationResponse>();
            return response;
        }
    }
}
