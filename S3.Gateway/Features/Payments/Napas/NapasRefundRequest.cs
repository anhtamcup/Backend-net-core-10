using MediatR;
using Microsoft.Extensions.Options;
using S3.Gateway.Data;
using S3.Gateway.Integrations.Payment;
using S3.Gateway.Integrations.Payment.Napas;

namespace S3.Gateway.Features.Payments.Napas
{
    public class NapasRefundRequest : IRequest<ResponseBase<NpRequestBase<NpRefundResponse>>>
    {
        public OriginalDataRequest OriginalData { get; set; } = new();
        public RefundDataRequest RefundData { get; set; } = new();
    }

    public class OriginalDataRequest
    {
        public string RefId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string OrigDateTime { get; set; } = string.Empty;
        public decimal OrigAmount { get; set; }
    }

    public class RefundDataRequest
    {
        public int RefundSequence { get; set; } = 1;
        public decimal RefundAmount { get; set; }
        public string RefundReason { get; set; } = string.Empty;
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

        private string CreateRefundId(int refundSequence, string transactionID, string refID)
        {
            // 01 ký tự đầu là chữ số cuối cùng của năm
            var part_1 = (DateTime.Now.Year % 10).ToString();

            // 01 ký tự tiếp theo điền là A
            var part_2 = "A";

            // 02 ký tự tiếp theo: giá trị duy nhất theo từng lần hoàn trả trên một giao dịch gốc
            var part_3 = refundSequence.ToString("D2");

            // "N" + alias.Substring(4, 3);
            var part_4 = "N" + transactionID.Substring(4, 3); ;

            var part_5 = refID.Substring(refID.Length - 8);

            var result = string.Format("{0}{1}{2}{3}{4}", part_1, part_2, part_3, part_4, part_5);

            if (result.Length != 16)
                throw new InvalidOperationException($"refundId sinh ra không đúng 16 ký tự");

            return result;
        }

        public async Task<ResponseBase<NpRequestBase<NpRefundResponse>>> Handle(NapasRefundRequest request, CancellationToken cancellationToken)
        {
            var origianalData = new NpOriginalDataRequest
            {
                Id = request.OriginalData.Id,
                RefId = request.OriginalData.RefId,
                OrigAmount = request.OriginalData.OrigAmount,
                OrigDateTime = request.OriginalData.OrigDateTime,
            };

            var refundId = CreateRefundId(
                request.RefundData.RefundSequence,
                origianalData.Id,
                origianalData.RefId);

            var refundData = new NpRefundDataRequest
            {
                RefundId = refundId,
                RefundAmount = request.RefundData.RefundAmount,
                RefundReason = request.RefundData.RefundReason
            };

            var napasRequest = new NpRefundRequest
            {
                OriginalData = origianalData,
                RefundData = refundData,
            };

            var response = await _napasAPI.Refund(napasRequest);
            var result = new ResponseBase<NpRequestBase<NpRefundResponse>>();
            result.IsSuccess = true;
            result.Data = response;
            return result;
        }
    }
}
