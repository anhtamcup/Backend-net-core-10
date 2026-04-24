using MediatR;
using Newtonsoft.Json;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.eKYC.Napas;

namespace S3.Gateway.Features.eKYC.Napas
{
    public class CreateMerchantRequest : NpCreateMerchantRequest, IRequest<ResponseBase<NpCreateMerchantResponse>>
    {
    }

    public class CreateMerchantRequestHandler : IRequestHandler<CreateMerchantRequest, ResponseBase<NpCreateMerchantResponse>>
    {
        private readonly NapasClient _napasClient;

        public CreateMerchantRequestHandler(NapasClient napasClient)
        {
            _napasClient = napasClient;
        }

        public async Task<ResponseBase<NpCreateMerchantResponse>> Handle(CreateMerchantRequest request, CancellationToken cancellationToken)
        {
            var response = new ResponseBase<NpCreateMerchantResponse>();
            try
            {
                var data = await _napasClient.CreateMerchant(request);
                response.IsSuccess = true;
                return response;
            }
            catch
            {
                response.Message = "Có lỗi xảy ra";
                return response;
            }
        }
    }
}