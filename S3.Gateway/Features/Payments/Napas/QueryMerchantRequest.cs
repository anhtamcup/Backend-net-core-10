using MediatR;
using S3.Gateway.Integrations.Napas;

namespace S3.Gateway.Features.Payments.Napas
{
    public class QueryMerchantRequest : NpQueryMerchantRequest, IRequest<ResponseBase<NpQueryMerchantResponse>>
    {
    }


    public class QueryMerchantRequestHandler : IRequestHandler<QueryMerchantRequest, ResponseBase<NpQueryMerchantResponse>>
    {
        private readonly NapasClient _napasClient;

        public QueryMerchantRequestHandler(NapasClient napasClient)
        {
            _napasClient = napasClient;
        }

        public async Task<ResponseBase<NpQueryMerchantResponse>> Handle(QueryMerchantRequest request, CancellationToken cancellationToken)
        {
            var response = new ResponseBase<NpQueryMerchantResponse>();
            try
            {
                var data = await _napasClient.QueryMerchant(request);
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