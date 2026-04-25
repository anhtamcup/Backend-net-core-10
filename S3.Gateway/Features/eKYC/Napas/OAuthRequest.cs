using MediatR;
using S3.Gateway.Data;
using S3.Gateway.Integrations.eKYC.Napas;

namespace S3.Gateway.Features.eKYC.Napas
{
    public class OAuthRequest : IRequest<NpOAuthResponse>
    {

    }

    public class OAuthRequestHandler : IRequestHandler<OAuthRequest, NpOAuthResponse>
    {
        private readonly NapasClient _napasClient;
        private readonly DBContext _dbContext;

        public OAuthRequestHandler(NapasClient napasClient, DBContext dbContext)
        {
            _napasClient = napasClient;
            _dbContext = dbContext;
        }

        public async Task<NpOAuthResponse> Handle(OAuthRequest request, CancellationToken cancellationToken)
        {
            var response = await _napasClient.OAuth();
            return response;
        }
    }
}