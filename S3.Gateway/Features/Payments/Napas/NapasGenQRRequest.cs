using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using S3.Gateway.Common;
using S3.Gateway.Data;
using S3.Gateway.Entities;
using S3.Gateway.Integrations.Ekyc.Napas;
using S3.Gateway.Integrations.Payment.Napas;
using System.Text.RegularExpressions;
using static S3.Gateway.Integrations.Payment.Napas.NapasQR;

namespace S3.Gateway.Features.Payments.Napas
{
    public class NapasGenQRRequest : IRequest<NapasGenQRResponse>
    {
        public class QRInfoObject
        {
            public string BankID { get; set; } = string.Empty;
            public string AccountNumber { get; set; } = string.Empty;
            public string ServiceType { get; set; } = string.Empty;
            public string PayloadFormatIndicator { get; set; } = string.Empty;
            public string PointOfInitiationMethod { get; set; } = string.Empty;
            public string TransactionCurrency { get; set; } = string.Empty;
            public string CountryCode { get; set; } = string.Empty;
            public string MerchantCategoryCode { get; set; } = string.Empty;
        }

        public QRInfoObject QRInfo { get; set; } = new();

        public string PartnerCode { get; set; } = string.Empty;
        public string RequestID { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BillNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }

    public class NapasGenQRResponse
    {
        public NapasGenQRResponse(string message = "")
        {
            Message = message;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
        public string QRCodeBase64 { get; set; }
        public string QRCodeData { get; set; }
        public string TransactionID { get; set; }
        public string TransDateTime { get; set; }
    }

    public static class NapasGenQRValidator
    {
        public static List<string> Validate(NapasGenQRRequest request)
        {
            var errors = new List<string>();

            if (request == null)
            {
                errors.Add("Request null");
                return errors;
            }

            // ===== Root =====
            if (string.IsNullOrWhiteSpace(request.PartnerCode))
                errors.Add("PartnerCode là bắt buộc");

            if (string.IsNullOrWhiteSpace(request.RequestID))
                errors.Add("RequestID là bắt buộc");

            if (request.Amount <= 0)
                errors.Add("Amount phải > 0");

            if (string.IsNullOrWhiteSpace(request.BillNumber))
            {
                errors.Add("BillNumber là bắt buộc");
            }

            if (!Regex.IsMatch(request.BillNumber, @"^[\x00-\x7F]+$"))
            {
                errors.Add("BillNumber chỉ hỗ trợ ký tự ASCII");
            }

            if (string.IsNullOrWhiteSpace(request.CallbackUrl))
                errors.Add("CallbackUrl là bắt buộc");

            if (!string.IsNullOrEmpty(request.Description))
            {
                if (request.Description.Length > 25)
                {
                    errors.Add("Description tối đa 25 ký tự");
                }

                // Chỉ cho phép ký tự ASCII (0-127)
                if (!Regex.IsMatch(request.Description, @"^[\x00-\x7F]+$"))
                {
                    errors.Add("Description chỉ hỗ trợ ký tự ASCII");
                }
            }

            //var db = DbContextManager.GetDbContext();
            //var napasBridgeTransaction = db.PaymentTransactions
            //    .Where(item => item.PartnerCode == request.PartnerCode && item.RequestID == request.RequestID)
            //    .FirstOrDefault();

            //if (napasBridgeTransaction != null)
            //{
            //    errors.Add("RequestID là định danh duy nhất mỗi yêu cầu");
            //}

            // ===== QRInfo =====
            var qrInfor = request.QRInfo;
            if (qrInfor == null)
            {
                errors.Add("QRInfo là bắt buộc");
                return errors;
            }

            ValidateQRInfo(qrInfor, errors);
            return errors;
        }

        private static void ValidateQRInfo(NapasGenQRRequest.QRInfoObject qr, List<string> errors)
        {
            if (!IsExactLength(qr.BankID, 6))
                errors.Add("BankID phải đúng 6 ký tự");

            if (!IsExactLength(qr.AccountNumber, 12))
                errors.Add("AccountNumber phải đúng 12 ký tự");

            if (!IsExactLength(qr.ServiceType, 8))
                errors.Add("ServiceType phải đúng 8 ký tự");

            if (!IsExactLength(qr.PayloadFormatIndicator, 2))
                errors.Add("PayloadFormatIndicator phải đúng 2 ký tự");

            if (!IsExactLength(qr.PointOfInitiationMethod, 2))
                errors.Add("PointOfInitiationMethod phải đúng 2 ký tự");

            if (!IsExactLength(qr.TransactionCurrency, 3))
                errors.Add("TransactionCurrency phải đúng 3 ký tự");

            if (!IsExactLength(qr.CountryCode, 2))
                errors.Add("CountryCode phải đúng 2 ký tự");

            if (!IsExactLength(qr.MerchantCategoryCode, 4))
                errors.Add("MerchantCategoryCode phải đúng 4 ký tự");
        }

        private static bool IsExactLength(string value, int length)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Length == length;
        }
    }

    public class NapasGenQRHandler : IRequestHandler<NapasGenQRRequest, NapasGenQRResponse>
    {
        private readonly DBContext _dbContext;
        private readonly NapasConfig _napasConfig;

        public NapasGenQRHandler(DBContext dbContext, IOptions<NapasConfig> options)
        {
            _dbContext = dbContext;
            _napasConfig = options.Value;
        }

        public async Task<NapasGenQRResponse> Handle(NapasGenQRRequest request, CancellationToken cancellationToken)
        {
            var response = new NapasGenQRResponse();
            var errors = NapasGenQRValidator.Validate(request);
            var now = DateTime.Now;

            if (errors.Any())
            {
                response.Message = errors[0];
                return response;
            }

            var callbackRouting = new CallbackRouting
            {
                RefID = request.RequestID,
                RequestID = RequestContext.RequestID,
                Target = Target3rd.Napas,
                PartnerCode = request.PartnerCode,
                CallbackUrl = request.CallbackUrl,
                RequestPayload = JsonConvert.SerializeObject(request),
                ActionHistory = "GEN QR",
            };

            _dbContext.CallbackRoutings.Add(callbackRouting);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var orderId = TransIdCodecHelper.Encode(callbackRouting.ID, TransIdCodecHelper.TransactionType.Forward);
            var qrInfor = request.QRInfo;
            var genQRRequest = new GenQRRequest
            {
                BankID = qrInfor.BankID,
                AccountNumber = qrInfor.AccountNumber + orderId,
                ServiceType = qrInfor.ServiceType,
                BillNumber = request.BillNumber,
                StoreLabel = request.PartnerCode,
                PurposeOfTransaction = request.Description,
                PayloadFormatIndicator = qrInfor.PayloadFormatIndicator,
                PointOfInitiationMethod = qrInfor.PointOfInitiationMethod,
                TransactionCurrency = qrInfor.TransactionCurrency,
                MerchantCategoryCode = qrInfor.MerchantCategoryCode,
                TransactionAmount = request.Amount,
                CountryCode = qrInfor.CountryCode,
            };

            var genQRResponse = NapasQR.GenQRCodeBase64(genQRRequest);

            var callbackRoutingLog = new CallbackRoutingLog
            {
                RequestID = callbackRouting.RequestID,
                Action = callbackRouting.ActionHistory,
                RequestPayload = JsonConvert.SerializeObject(genQRRequest),
                ResponsePayload = JsonConvert.SerializeObject(genQRResponse),
                IsSuccess = genQRResponse.Success
            };

            _dbContext.CallbackRoutingLogs.Add(callbackRoutingLog);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (genQRResponse.Success == false)
            {
                response.Message = "Gen QR thất bại";
                return response;
            }

            response.QRCodeBase64 = genQRResponse.QRCodeBase64;
            response.QRCodeData = genQRResponse.QRCodeData;
            response.TransDateTime = now.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00";
            response.TransactionID = genQRRequest.AccountNumber;
            response.Success = true;
            return response;
        }
    }
}
