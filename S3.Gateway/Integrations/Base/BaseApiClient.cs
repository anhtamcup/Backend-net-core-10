using Newtonsoft.Json;
using S3.Gateway.Entities;
using S3.Gateway.Features.Logs;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace S3.Gateway.Integrations.Base
{
    public class BaseApiClient
    {
        private readonly ILogService _logService;

        public BaseApiClient(ILogService logService)
        {
            _logService = logService;
        }

        public async Task<T> PostTAsync<T>(
            string url,
            object data,
            string token = "",
            Func<Task<string>>? refreshToken = null,
            Dictionary<string, string>? headers = null)
        {
            var stopwatch = Stopwatch.StartNew();
            var log = new ApiLog
            {
                Url = url,
                Method = HttpMethod.Post.ToString(),
                CreatedAt = DateTime.Now
            };

            try
            {
                var handler = new WinHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.All
                };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using var client = new HttpClient(handler);

                if (string.IsNullOrWhiteSpace(token) == false)
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                client.DefaultRequestHeaders.Accept.ParseAdd("*/*");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("br");
                client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.53.0");

                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                async Task<(HttpStatusCode StatusCode, string Content)> SendRequestAsync(bool retry = false)
                {
                    HttpContent content;
                    if (data is IEnumerable<KeyValuePair<string, string>> formData)
                    {
                        content = new FormUrlEncodedContent(formData);
                    }
                    else
                    {
                        content = new StringContent(
                            data is string s ? s : JsonConvert.SerializeObject(data),
                            Encoding.UTF8,
                            "application/json");
                    }

                    var requestBody = await content.ReadAsStringAsync();
                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (retry)
                    {
                        log.RequestBody2 = requestBody;
                        log.ResponseBody2 = responseString;
                    }
                    else
                    {
                        log.RequestBody = requestBody;
                        log.ResponseBody = responseString;
                    }

                    return (response.StatusCode, responseString);
                }

                var response = await SendRequestAsync();

                if (response.StatusCode == HttpStatusCode.Unauthorized && refreshToken != null)
                {
                    // Refresh token one time
                    var newToken = await refreshToken();
                    if (string.IsNullOrWhiteSpace(newToken) == false)
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                        response = await SendRequestAsync(true);
                    }
                }

                log.StatusCode = (int)response.StatusCode;
                log.IsSuccess = response.StatusCode == HttpStatusCode.OK;

                var result = JsonConvert.DeserializeObject<T>(response.Content);
                return result;
            }
            catch (Exception ex)
            {
                log.ErrorMessage = ex.ToString();
                return default(T);
            }
            finally
            {
                stopwatch.Stop();
                log.Duration = (int)stopwatch.ElapsedMilliseconds;

                _ = _logService.SaveAsync(log);
            }
        }
    }
}