using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Integrations.Base;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace S3.Gateway.Integrations.Payment.Napas
{
    public class NapasApi
    {
        private readonly BaseApiClient _baseClient;
        private readonly PaymentConfig _pConfig;

        public NapasApi(
            BaseApiClient baseClient,
            IOptions<PaymentConfig> options)
        {
            _baseClient = baseClient;
            _pConfig = options.Value;
        }

        private async Task<T> CallApi<T>(string url, object request, string token = null)
        {
            var payload = Utility.SerializeObjectLowerCase(request);
            var privateKeyPath = Path.Combine(AppContext.BaseDirectory, _pConfig.Napas.PrivateKey);
            var signaturePayload = RSASignatureService.SignMessage(payload, privateKeyPath);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            var certificatePath = Path.Combine(AppContext.BaseDirectory, _pConfig.Napas.SSL);
            var certificate = new X509Certificate2(
                certificatePath,
                _pConfig.Napas.SSLPassword,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            var result = await _baseClient.PostTAsync<T>(
                url,
                request,
                token: token,
                clientCert: certificate
                );

            return result;
        }

        private async Task<NpOAuthResponse> OAuth()
        {
            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>(Constants.CONST_KEY_GRANT_TYPE, _pConfig.Napas.GrantType));
            collection.Add(new KeyValuePair<string, string>(Constants.CONST_KEY_CLIENT_ID, _pConfig.Napas.ClientId));
            collection.Add(new KeyValuePair<string, string>(Constants.CONST_KEY_CLIENT_SECRET, _pConfig.Napas.ClientSecret));

            var oathResponse = await CallApi<NpOAuthResponse>(_pConfig.Napas.OauthURL, collection);
            return oathResponse;
        }

        public async Task<NpRequestBase<NpInvestigationResponse>> Investigation(NpInvestigationRequest payload)
        {
            var now = DateTime.UtcNow;
            var senderID = _pConfig.Napas.SenderId;
            var receiverId = _pConfig.Napas.ReceiverId;
            var refID = now.ToString("ddHHmmssfff").PadLeft(12, '0');
            var creationDateTime = now.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00";
            var senderReference = now.ToString("yyyyMMdd") + senderID + Constants.SERVICE_ID_INVESTIGATION + refID;

            payload.CaseId = senderReference;
            payload.CreationDateTime = creationDateTime;

            var header = new NpHeader
            {
                MessageIdentifier = Constants.MESSAGE_IDENTIFIER_INVEST_REQUEST,
                CreationDateTime = creationDateTime,
                SenderReference = senderReference,
                SenderId = senderID,
                ReceiverId = receiverId,
                Signature = RSASignatureService.SignMessage(Utility.SerializeObjectLowerCase(payload), _pConfig.Napas.PrivateKey)
            };

            var requestNapas = new NpRequestBase<NpInvestigationRequest>()
            {
                Header = header,
                Payload = payload,
            };

            var oauth = await OAuth();
            if (oauth == null || string.IsNullOrWhiteSpace(oauth.AccessToken))
            {
                return null;
            }

            var data = Utility.SerializeObjectLowerCase(requestNapas);
            var result = await CallApi<NpRequestBase<NpInvestigationResponse>>(_pConfig.Napas.InvestigationURL, data, oauth.AccessToken);
            return result;
        }

        public static string GenerateRefundId(
        int refundSequence,
        string alias,
        string originalTransRef)
        {
            if (refundSequence < 1 || refundSequence > 99)
                throw new ArgumentOutOfRangeException(nameof(refundSequence), "refundSequence phải trong khoảng 1-99");

            if (string.IsNullOrEmpty(alias) || alias.Length < 7)
                throw new ArgumentException("Alias phải có ít nhất 7 ký tự", nameof(alias));

            if (string.IsNullOrEmpty(originalTransRef) || originalTransRef.Length < 8)
                throw new ArgumentException("originalTransRef phải có ít nhất 8 ký tự", nameof(originalTransRef));

            DateTime targetDate = DateTime.Now;

            // 1. Chữ số cuối cùng của năm
            string yearLastDigit = (targetDate.Year % 10).ToString();

            // 2. Ký tự "A"
            const string fixedA = "A";

            // 3. 02 ký tự: số thứ tự lần hoàn trả (01, 02, ...)
            string sequencePart = refundSequence.ToString("D2");

            // 4. 04 ký tự: "N" + 3 ký tự từ vị trí thứ 5 đến 7 của Alias (index 4..6, 0-based)
            string merchantCode = "N" + alias.Substring(4, 3);

            // 5. 08 ký tự cuối của transRef gốc
            string last8OfTransRef = originalTransRef.Substring(originalTransRef.Length - 8);

            string refundId = yearLastDigit + fixedA + sequencePart + merchantCode + last8OfTransRef;

            // Kiểm tra lại đúng 16 ký tự
            if (refundId.Length != 16)
                throw new InvalidOperationException($"refundId sinh ra không đúng 16 ký tự: {refundId} (length={refundId.Length})");

            return refundId;
        }

        public async Task<NpRequestBase<NpRefundResponse>> Refund(NpRefundRequest payload)
        {
            var now = DateTime.UtcNow;
            var senderID = _pConfig.Napas.SenderId;
            var receiverId = _pConfig.Napas.ReceiverId;
            var refID = now.ToString("ddHHmmssfff").PadLeft(12, '0');
            var creationDateTime = now.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00";
            var senderReference = now.ToString("yyyyMMdd") + senderID + Constants.SERVICE_ID_REFUND + refID;

            payload.CaseId = senderReference;
            payload.CreationDateTime = creationDateTime;
            payload.RefundData.RefundId = GenerateRefundId(
                1,
                "NPCFCFB",
                payload.OriginalData.RefId);

            var header = new NpHeader
            {
                MessageIdentifier = Constants.MESSAGE_IDENTIFIER_REFUND_REQUEST,
                CreationDateTime = creationDateTime,
                SenderReference = senderReference,
                SenderId = senderID,
                ReceiverId = receiverId,
                Signature = RSASignatureService.SignMessage(Utility.SerializeObjectLowerCase(payload), _pConfig.Napas.PrivateKey)
            };

            var requestNapas = new NpRequestBase<NpRefundRequest>()
            {
                Header = header,
                Payload = payload,
            };

            var oauth = await OAuth();
            if (oauth == null || string.IsNullOrWhiteSpace(oauth.AccessToken))
            {
                return null;
            }

            var data = Utility.SerializeObjectLowerCase(requestNapas);
            var result = await CallApi<NpRequestBase<NpRefundResponse>>(_pConfig.Napas.RefundURL, data, oauth.AccessToken);
            return result;
        }
    }
}
