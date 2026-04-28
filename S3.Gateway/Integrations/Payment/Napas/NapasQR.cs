using QRCoder;
using System.Drawing.Imaging;

namespace S3.Gateway.Integrations.Payment.Napas
{
    public static class NapasQR
    {
        public class GenQRRequest
        {
            public string BankID { get; set; } = string.Empty;
            public string AccountNumber { get; set; } = string.Empty;
            public string ServiceType { get; set; } = string.Empty;
            public string BillNumber { get; set; } = string.Empty;
            public string StoreLabel { get; set; } = string.Empty;
            public string PurposeOfTransaction { get; set; } = string.Empty;
            public string PayloadFormatIndicator { get; set; } = string.Empty;
            public string PointOfInitiationMethod { get; set; } = string.Empty;
            public string TransactionCurrency { get; set; } = string.Empty;
            public string MerchantCategoryCode { get; set; } = string.Empty;
            public decimal TransactionAmount { get; set; }
            public string CountryCode { get; set; } = string.Empty;
        }

        public class GenQRResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string QRCodeBase64 { get; set; } = string.Empty;
            public string QRCodeData { get; set; } = string.Empty;
        }

        public static GenQRResponse GenQRCodeBase64(GenQRRequest request)
        {
            var result = new GenQRResponse();
            try
            {
                var qrDataString = GenQRStringData(request);
                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrDataString, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);

                using (MemoryStream ms = new MemoryStream())
                {
                    // Lưu ảnh QR vào memory stream (PNG)
                    qrCodeImage.Save(ms, ImageFormat.Png);

                    byte[] imageBytes = ms.ToArray();

                    // Convert sang Base64
                    var base64 = Convert.ToBase64String(imageBytes);
                    result.QRCodeBase64 = base64;
                    result.QRCodeData = qrDataString;
                    result.Success = true;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        private static string GenQRStringData(GenQRRequest request)
        {
            var consumerAccount = new ConsumerAccount();
            consumerAccount.BankID.Value = request.BankID;
            consumerAccount.AccountNumber.Value = request.AccountNumber;
            consumerAccount.ServiceType.Value = request.ServiceType;
            var consumerAccountPayload = consumerAccount.GetPayload();

            var dataFieldTemplate = new DataFieldTemplate();
            dataFieldTemplate.BillNumber.Value = request.BillNumber;
            dataFieldTemplate.StoreLabel.Value = request.StoreLabel;
            dataFieldTemplate.PurposeOfTransaction.Value = request.PurposeOfTransaction;

            var QRIBFTTA = new QRIBFTTA();
            QRIBFTTA.PayloadFormatIndicator.Value = request.PayloadFormatIndicator;
            QRIBFTTA.PointOfInitiationMethod.Value = request.PointOfInitiationMethod;
            QRIBFTTA.ConsumerAccountInformation.Value = consumerAccountPayload;
            QRIBFTTA.TransactionCurrency.Value = request.TransactionCurrency;
            QRIBFTTA.MerchantCategoryCode.Value = request.MerchantCategoryCode;
            QRIBFTTA.TransactionAmount.Value = request.TransactionAmount.ToString();
            QRIBFTTA.CountryCode.Value = request.CountryCode;
            QRIBFTTA.AdditionalDataFieldTemplate.Value = dataFieldTemplate.GetPayload();
            var qrDataString = QRIBFTTA.GetPayload();
            return qrDataString;
        }
    }
}
