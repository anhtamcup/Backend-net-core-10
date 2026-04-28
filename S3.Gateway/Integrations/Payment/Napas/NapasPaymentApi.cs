using Microsoft.Extensions.Options;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Ekyc.Napas;

namespace S3.Gateway.Integrations.Payment.Napas
{
    public class NapasClient
    {
        private readonly BaseApiClient _baseClient;
        private readonly PaymentConfig _pConfig;

        public NapasClient(
            BaseApiClient baseClient,
            IOptions<PaymentConfig> options,
            NapasTokenService tokenService)
        {
            _baseClient = baseClient;
            _pConfig = options.Value;
        }

        //private async Task<T> CallApi<T>(string url, object request)
        //{
        //    var payload = RSASignatureService.SerializeObject(request);
        //    var privateKeyPath = Path.Combine(AppContext.BaseDirectory, _napasConfig.Ekyc.PrivateKey);
        //    var signaturePayload = RSASignatureService.SignMessage(payload, privateKeyPath);
        //    var headers = new Dictionary<string, string>
        //    {
        //        { "Application", _napasConfig.Ekyc.Application },
        //        { "signature", signaturePayload }
        //    };

        //    return await _baseClient.PostTAsync<T>(
        //        url,
        //        payload,
        //        await GetAccessToken(),
        //        GetRefreshToken,
        //        headers);
        //}

        //private async Task<string> GetAccessToken()
        //{
        //    return await _tokenService.GetToken(OAuth);
        //}

        //private async Task<string> GetRefreshToken()
        //{
        //    _tokenService.ClearToken();
        //    return await _tokenService.GetToken(OAuth);
        //}

        //public async Task<NpOAuthResponse> OAuth()
        //{
        //    var oauthBody = new List<KeyValuePair<string, string>>
        //    {
        //        new(NapasConstants.CONST_KEY_GRANT_TYPE, _napasConfig.GrantType),
        //        new(NapasConstants.CONST_KEY_CLIENT_ID, _napasConfig.ClientId),
        //        new(NapasConstants.CONST_KEY_CLIENT_SECRET, _napasConfig.ClientSecret)
        //    };

        //    var result = await _baseClient.PostTAsync<NpOAuthResponse>(
        //        _napasConfig.OauthURL,
        //        oauthBody);

        //    return result;
        //}

        //public async Task<NpRequestBase<NpInvestigationResponse>> GetDeepLink(NpInvestigationRequest request)
        //{
        //    var now = DateTime.UtcNow;
        //    var refID = now.ToString("ddHHmmssfff").PadLeft(12, '0');
        //    var creationDateTime = now.ToString("yyyy-MM-ddTHH:mm:ss") + "+07:00";
        //    var senderReference = now.ToString("yyyyMMdd") + NapasConstants.SENDER_ID + NapasConstants.SERVICE_ID_INVESTIGATION + refID;

        //    payload.caseId = senderReference;
        //    payload.creationDateTime = creationDateTime;
        //}
    }
}
