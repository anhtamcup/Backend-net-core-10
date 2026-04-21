using Microsoft.Extensions.Options;
using S3.Gateway.Integrations.Base;
using System.Net.Http.Headers;

namespace S3.Gateway.Integrations.Napas
{
    public class NapasClient
    {
        private readonly HttpClient _httpClient;
        private readonly BaseApiClient _baseClient;
        private readonly NapasConfig _napasConfig;

        public NapasClient(
            HttpClient httpClient,
            BaseApiClient baseClient,
            IOptions<NapasConfig> options)
        {
            _httpClient = httpClient;
            _baseClient = baseClient;
            _napasConfig = options.Value;
        }

        private async Task<NpOAuthResponse> OAuth()
        {
            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new KeyValuePair<string, string>(NapasConstants.CONST_KEY_GRANT_TYPE, _napasConfig.GrantType));
            collection.Add(new KeyValuePair<string, string>(NapasConstants.CONST_KEY_CLIENT_ID, _napasConfig.ClientId));
            collection.Add(new KeyValuePair<string, string>(NapasConstants.CONST_KEY_CLIENT_SECRET, _napasConfig.ClientSecret));
            var postData = _baseClient.ConvertToFormUrlEncoded(collection);
            var request = new HttpRequestMessage(HttpMethod.Post, _napasConfig.OauthURL)
            {
                Content = postData
            };

            var response = await _baseClient.SendAsync<NpOAuthResponse>(_httpClient, request);
            return response ?? new NpOAuthResponse();
        }

        private async Task<HttpRequestMessage> CreateHttpRequestMessage(string enpoint, object request)
        {
            var httpRequest = new HttpRequestMessage(
                HttpMethod.Post,
                enpoint);

            var oAuth = await OAuth();
            httpRequest.Content = JsonContent.Create(request);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oAuth.AccessToken);
            return httpRequest;
        }

        public async Task<NpGetDeepLinkResponse> GetDeepLink(NpGetDeepLinkRequest request)
        {
            var httpRequest = await CreateHttpRequestMessage(_napasConfig.GetDeepLinkURL, request);

            var result = await _baseClient.SendAsync<NpGetDeepLinkResponse>(
                _httpClient,
                httpRequest);

            return result;
        }

        public async Task<NpCreateMerchantResponse> CreateMerchant(NpCreateMerchantRequest request)
        {
            var httpRequest = await CreateHttpRequestMessage(_napasConfig.CreateMerchantURL, request);
            var result = await _baseClient.SendAsync<NpCreateMerchantResponse>(
                _httpClient,
                httpRequest);

            return result;
        }

        public async Task<NpUpdateMerchantResponse> UpdateMerchant(NpUpdateMerchantRequest request)
        {
            var httpRequest = await CreateHttpRequestMessage(_napasConfig.UpdateMerchantURL, request);
            var result = await _baseClient.SendAsync<NpUpdateMerchantResponse>(
                _httpClient,
                httpRequest);

            return result;
        }

        public async Task<NpQueryMerchantResponse> QueryMerchant(NpQueryMerchantRequest request)
        {
            var httpRequest = await CreateHttpRequestMessage(_napasConfig.QueryMerchantURL, request);
            var result = await _baseClient.SendAsync<NpQueryMerchantResponse>(
                _httpClient,
                httpRequest);

            return result;
        }

        public async Task<NpUpdateBankAccountResponse> UpdateBankAccount(NpUpdateBankAccountRequest request)
        {
            var httpRequest = await CreateHttpRequestMessage(_napasConfig.UpdateBankAccountURL, request);
            var result = await _baseClient.SendAsync<NpUpdateBankAccountResponse>(
                _httpClient,
                httpRequest);

            return result;
        }
    }
}
