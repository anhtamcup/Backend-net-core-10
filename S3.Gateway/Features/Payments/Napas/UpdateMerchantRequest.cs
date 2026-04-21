using MediatR;
using S3.Gateway.Integrations.Napas;

namespace S3.Gateway.Features.Payments.Napas
{
    public class UpdateMerchantRequest : NpUpdateMerchantRequest, IRequest<ResponseBase<NpUpdateMerchantResponse>>
    {
    }


    public class UpdateMerchantRequestHandler : IRequestHandler<UpdateMerchantRequest, ResponseBase<NpUpdateMerchantResponse>>
    {
        private readonly NapasClient _napasClient;

        public UpdateMerchantRequestHandler(NapasClient napasClient)
        {
            _napasClient = napasClient;
        }

        public async Task<ResponseBase<NpUpdateMerchantResponse>> Handle(UpdateMerchantRequest request, CancellationToken cancellationToken)
        {
            var response = new ResponseBase<NpUpdateMerchantResponse>();
            try
            {
                var data = await _napasClient.UpdateMerchant(request);
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