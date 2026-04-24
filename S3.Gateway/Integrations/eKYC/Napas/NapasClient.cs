using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using S3.Gateway.Integrations.Base;
using System.Net;
using System.Net.Http.Headers;

namespace S3.Gateway.Integrations.eKYC.Napas
{
    public class NapasClient
    {
        private readonly HttpClient _httpClient;
        private readonly BaseApiClient _baseClient;
        private readonly NapasConfig _napasConfig;
        private readonly NapasTokenService _tokenService;

        public NapasClient(
            HttpClient httpClient,
            BaseApiClient baseClient,
            IOptions<NapasConfig> options,
            NapasTokenService tokenService)
        {
            _httpClient = httpClient;
            _baseClient = baseClient;
            _napasConfig = options.Value;
            _tokenService = tokenService;
        }

        private async Task<string> GetAccessToken()
        {
            return await _tokenService.GetToken(OAuth);
        }

        public async Task<NpOAuthResponse> OAuth()
        {
            var collection = new List<KeyValuePair<string, string>>
            {
                new(NapasConstants.CONST_KEY_GRANT_TYPE, _napasConfig.GrantType),
                new(NapasConstants.CONST_KEY_CLIENT_ID, _napasConfig.ClientId),
                new(NapasConstants.CONST_KEY_CLIENT_SECRET, _napasConfig.ClientSecret)
            };

            var postData = _baseClient.ConvertToFormUrlEncoded(collection);

            var request = new HttpRequestMessage(HttpMethod.Post, _napasConfig.OauthURL)
            {
                Content = postData
            };

            var response = await _baseClient.SendAsync<NpOAuthResponse>(_httpClient, request);

            return response ?? new NpOAuthResponse();
        }

        private async Task<HttpRequestMessage> CreateHttpRequestMessage(string endpoint, object request)
        {
            //var accessToken = await GetAccessToken();
            var oauth = await OAuth();
            var accessToken = oauth.AccessToken;
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);

            httpRequest.Content = JsonContent.Create(request);

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var privateKeyPath = Path.Combine(
                AppContext.BaseDirectory,
                _napasConfig.Key.eKYC.PrivateKey
            );

            var payload = JsonConvert.SerializeObject(request);
            var signature = RsaSignatureService.Sign(
                payload,
                privateKeyPath,
                _napasConfig.Key.eKYC.Password
            );

            httpRequest.Headers.Add("signature", signature);
            httpRequest.Headers.Add("Application", "json");

            return httpRequest;
        }

        private async Task<T> SendWithRetry<T>(string endpoint, object request)
        {
            var httpRequest = await CreateHttpRequestMessage(endpoint, request);

            var response = await _httpClient.SendAsync(httpRequest);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return await DeserializeResponse<T>(response);
            }

            // Token hết hạn → xóa cache
            _tokenService.ClearToken();

            // Retry với token mới
            httpRequest = await CreateHttpRequestMessage(endpoint, request);

            response = await _httpClient.SendAsync(httpRequest);

            return await DeserializeResponse<T>(response);
        }

        private async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseBody))
                return default;

            return JsonConvert.DeserializeObject<T>(responseBody);
        }

        public async Task<NpGetDeepLinkResponse> GetDeepLink(NpGetDeepLinkRequest request)
        {
            return await SendWithRetry<NpGetDeepLinkResponse>(
                _napasConfig.GetDeepLinkURL,
                request);
        }

        public async Task<NpCreateMerchantResponse> CreateMerchant(NpCreateMerchantRequest request)
        {
            return await SendWithRetry<NpCreateMerchantResponse>(
                _napasConfig.CreateMerchantURL,
                request);
        }

        public async Task<NpUpdateMerchantResponse> UpdateMerchant(NpUpdateMerchantRequest request)
        {
            return await SendWithRetry<NpUpdateMerchantResponse>(
                _napasConfig.UpdateMerchantURL,
                request);
        }

        public async Task<NpQueryMerchantResponse> QueryMerchant(NpQueryMerchantRequest request)
        {
            return await SendWithRetry<NpQueryMerchantResponse>(
                _napasConfig.QueryMerchantURL,
                request);
        }

        public async Task<NpUpdateBankAccountResponse> UpdateBankAccount(NpUpdateBankAccountRequest request)
        {
            return await SendWithRetry<NpUpdateBankAccountResponse>(
                _napasConfig.UpdateBankAccountURL,
                request);
        }
    }
}