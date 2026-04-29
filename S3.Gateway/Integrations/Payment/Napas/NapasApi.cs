using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using S3.Gateway.Common;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Ekyc.Napas;
using System.Net;
using System.Reflection.PortableExecutable;
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
                MessageIdentifier = "investrequest",
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
    }
}
