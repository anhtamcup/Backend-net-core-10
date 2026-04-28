using Microsoft.Extensions.Options;
using S3.Gateway.Common;
using S3.Gateway.Integrations.Base;

namespace S3.Gateway.Integrations.Ekyc.Napas
{
    public class NapasClient
    {
        private readonly BaseApiClient _baseClient;
        private readonly EkycConfig _eConfig;
        private readonly NapasTokenService _tokenService;

        public NapasClient(
            BaseApiClient baseClient,
            IOptions<EkycConfig> options,
            NapasTokenService tokenService)
        {
            _baseClient = baseClient;
            _eConfig = options.Value;
            _tokenService = tokenService;
        }

        private async Task<T> CallApi<T>(string url, object request)
        {
            var payload = Utility.SerializeObjectLowerCase(request);
            var privateKeyPath = Path.Combine(AppContext.BaseDirectory, _eConfig.Napas.PrivateKey);
            var signaturePayload = RSASignatureService.SignMessage(payload, privateKeyPath);
            var headers = new Dictionary<string, string>
            {
                { "Application", _eConfig.Napas.Application },
                { "signature", signaturePayload }
            };

            return await _baseClient.PostTAsync<T>(
                url,
                payload,
                await GetAccessToken(),
                GetRefreshToken,
                headers);
        }

        private async Task<string> GetAccessToken()
        {
            return await _tokenService.GetToken(OAuth);
        }

        private async Task<string> GetRefreshToken()
        {
            _tokenService.ClearToken();
            return await _tokenService.GetToken(OAuth);
        }

        public async Task<NpOAuthResponse> OAuth()
        {
            var oauthBody = new List<KeyValuePair<string, string>>
            {
                new(NapasConstants.CONST_KEY_GRANT_TYPE, _eConfig.Napas.GrantType),
                new(NapasConstants.CONST_KEY_CLIENT_ID, _eConfig.Napas.ClientId),
                new(NapasConstants.CONST_KEY_CLIENT_SECRET, _eConfig.Napas.ClientSecret)
            };

            var result = await _baseClient.PostTAsync<NpOAuthResponse>(
                _eConfig.Napas.OauthURL,
                oauthBody);

            return result;
        }

        public async Task<NpGetDeepLinkResponse> GetDeepLink(NpGetDeepLinkRequest request)
        {
            var result = await CallApi<NpGetDeepLinkResponse>(_eConfig.Napas.GetDeepLinkURL, request);
            return result;
        }

        public async Task<NpCreateMerchantResponse> CreateMerchant(NpCreateMerchantRequest request)
        {
            var result = await CallApi<NpCreateMerchantResponse>(_eConfig.Napas.CreateMerchantURL, request);
            return result;
        }

        public async Task<NpUpdateMerchantResponse> UpdateMerchant(NpUpdateMerchantRequest request)
        {
            var result = await CallApi<NpUpdateMerchantResponse>(_eConfig.Napas.UpdateMerchantURL, request);
            return result;
        }

        public async Task<NpQueryMerchantResponse> QueryMerchant(NpQueryMerchantRequest request)
        {
            var result = await CallApi<NpQueryMerchantResponse>(_eConfig.Napas.QueryMerchantURL, request);
            return result;
        }

        public async Task<NpUpdateBankAccountResponse> UpdateBankAccount(NpUpdateBankAccountRequest request)
        {
            var result = await CallApi<NpUpdateBankAccountResponse>(_eConfig.Napas.UpdateBankAccountURL, request);
            return result;
        }
    }
}