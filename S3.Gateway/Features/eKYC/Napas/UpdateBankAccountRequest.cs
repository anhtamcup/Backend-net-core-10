using MediatR;
using S3.Gateway.Integrations.eKYC.Napas;

namespace S3.Gateway.Features.eKYC.Napas
{
    public class UpdateBankAccountRequest : NpUpdateBankAccountRequest, IRequest<ResponseBase<NpUpdateBankAccountResponse>>
    {
    }

    public class UpdateBankAccountRequestHandler : IRequestHandler<UpdateBankAccountRequest, ResponseBase<NpUpdateBankAccountResponse>>
    {
        private readonly NapasClient _napasClient;

        public UpdateBankAccountRequestHandler(NapasClient napasClient)
        {
            _napasClient = napasClient;
        }

        public async Task<ResponseBase<NpUpdateBankAccountResponse>> Handle(UpdateBankAccountRequest request, CancellationToken cancellationToken)
        {
            var response = new ResponseBase<NpUpdateBankAccountResponse>();
            try
            {
                var data = await _napasClient.UpdateBankAccount(request);
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