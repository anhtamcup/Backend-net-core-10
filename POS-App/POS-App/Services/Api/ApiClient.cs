using POS_App.Config;
using POS_App.Models.Response;
using POS_App.Services.Interfaces;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace POS_App.Services.Api
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _client;
        private readonly AppConfig _config;

        public ApiClient()
        {
            _config = App.Config;
            _config = App.Config;

            _client = new HttpClient
            {
                BaseAddress = new Uri(_config.ApiSettings.BaseUrl),
                Timeout = TimeSpan.FromSeconds(_config.ApiSettings.Timeout)
            };
        }

        public async Task<T?> PostAsync<T>(
            string url,
            object data)
            where T : ResponseBase, new()
        {
            try
            {
                var token = TokenStorageService.GetAccessToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            token);
                }
                else
                {
                    _client.DefaultRequestHeaders.Authorization = null;
                }

                var response =
                    await _client
                        .PostAsJsonAsync(
                            url,
                            data);

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    return new T
                    {
                        IsSuccess = false,
                        Message = "Sai thông tin tài khoản"
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new T
                    {
                        IsSuccess = false,
                        Message =
                            $"API lỗi: {(int)response.StatusCode}"
                    };
                }

                var result =
                    await response.Content
                        .ReadFromJsonAsync<T>();

                return result;
            }
            catch (Exception ex)
            {
                return new T
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<T?> GetAsync<T>(
            string url)
            where T : ResponseBase, new()
        {
            try
            {
                var token = TokenStorageService.GetAccessToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            token);
                }
                else
                {
                    _client.DefaultRequestHeaders.Authorization = null;
                }

                var response =
                    await _client.GetAsync(url);

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    return new T
                    {
                        IsSuccess = false,
                        Message = "Phiên đăng nhập hết hạn"
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    return new T
                    {
                        IsSuccess = false,
                        Message =
                            $"API lỗi: {(int)response.StatusCode}"
                    };
                }

                return await response.Content
                    .ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                return new T
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
