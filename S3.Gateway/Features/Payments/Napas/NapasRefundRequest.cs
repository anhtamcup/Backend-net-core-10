using MediatR;
using Microsoft.Extensions.Options;
using S3.Gateway.Data;
using S3.Gateway.Integrations.Payment;
using S3.Gateway.Integrations.Payment.Napas;

namespace S3.Gateway.Features.Payments.Napas
{
    public class NapasRefundRequest: IRequest<ResponseBase<NpRequestBase<NpRefundResponse>>>
    {
        public NpOriginalDataRequest OriginalData { get; set; } = new NpOriginalDataRequest();
        public NpRefundDataRequest RefundData { get; set; } = new NpRefundDataRequest();
    }

    public class NapasRefundHandler : IRequestHandler<NapasRefundRequest, ResponseBase<NpRequestBase<NpRefundResponse>>>
    {
        private readonly DBContext _dbContext;
        private readonly PaymentConfig _pConfig;
        private readonly NapasApi _napasAPI;

        public NapasRefundHandler(DBContext dbContext, IOptions<PaymentConfig> options, NapasApi napasAPI)
        {
            _dbContext = dbContext;
            _pConfig = options.Value;
            _napasAPI = napasAPI;
        }

        public async Task<ResponseBase<NpRequestBase<NpRefundResponse>>> Handle(NapasRefundRequest request, CancellationToken cancellationToken)
        {
            var napasRequest = new NpRefundRequest
            {
                OriginalData = request.OriginalData,
                RefundData = request.RefundData,
            };

            var response = await _napasAPI.Refund(napasRequest);
            var result = new ResponseBase<NpRequestBase<NpRefundResponse>>();
            result.IsSuccess = true;
            result.Data = response;
            return result;
        }
    }
}
