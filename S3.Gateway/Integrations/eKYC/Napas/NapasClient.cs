using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using S3.Gateway.Integrations.Base;

namespace S3.Gateway.Integrations.eKYC.Napas
{
    public class NapasClient
    {
        private readonly BaseApiClient _baseClient;
        private readonly NapasConfig _napasConfig;
        private readonly NapasTokenService _tokenService;

        public NapasClient(
            BaseApiClient baseClient,
            IOptions<NapasConfig> options,
            NapasTokenService tokenService)
        {
            _baseClient = baseClient;
            _napasConfig = options.Value;
            _tokenService = tokenService;
        }

        private async Task<T> CallApi<T>(string url, object request)
        {
            var payload = JsonConvert.SerializeObject(
                request,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });

            var headers = new Dictionary<string, string>
            {
                { "Application", "json" },
                { "signature", "xxx" }
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
                new(NapasConstants.CONST_KEY_GRANT_TYPE, _napasConfig.GrantType),
                new(NapasConstants.CONST_KEY_CLIENT_ID, _napasConfig.ClientId),
                new(NapasConstants.CONST_KEY_CLIENT_SECRET, _napasConfig.ClientSecret)
            };

            var result = await _baseClient.PostTAsync<NpOAuthResponse>(
                _napasConfig.OauthURL,
                oauthBody);

            return result;
        }

        public async Task<NpGetDeepLinkResponse> GetDeepLink(NpGetDeepLinkRequest request)
        {
            var result = await CallApi<NpGetDeepLinkResponse>(_napasConfig.GetDeepLinkURL, request);
            return result;
        }

        public async Task<NpCreateMerchantResponse> CreateMerchant(NpCreateMerchantRequest request)
        {
            var result = await CallApi<NpCreateMerchantResponse>(_napasConfig.CreateMerchantURL, request);
            return result;
        }

        public async Task<NpUpdateMerchantResponse> UpdateMerchant(NpUpdateMerchantRequest request)
        {
            var result = await CallApi<NpUpdateMerchantResponse>(_napasConfig.UpdateMerchantURL, request);
            return result;
        }

        public async Task<NpQueryMerchantResponse> QueryMerchant(NpQueryMerchantRequest request)
        {
            var result = await CallApi<NpQueryMerchantResponse>(_napasConfig.QueryMerchantURL, request);
            return result;
        }

        public async Task<NpUpdateBankAccountResponse> UpdateBankAccount(NpUpdateBankAccountRequest request)
        {
            var result = await CallApi<NpUpdateBankAccountResponse>(_napasConfig.UpdateBankAccountURL, request);
            return result;
        }
    }
}