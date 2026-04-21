using S3.Gateway.Entities;
using S3.Gateway.Features.Logs;
using System.Diagnostics;
using System.Text.Json;

namespace S3.Gateway.Integrations.Base
{
    public class BaseApiClient
    {
        private readonly ILogService _logService;

        public BaseApiClient(ILogService logService)
        {
            _logService = logService;
        }

        public HttpContent ConvertToFormUrlEncoded(
            IEnumerable<KeyValuePair<string, string>> data)
        {
            return new FormUrlEncodedContent(data);
        }

        public async Task<T?> SendAsync<T>(
            HttpClient client,
            HttpRequestMessage request)
        {
            var requestBody = request.Content != null
                ? await request.Content.ReadAsStringAsync()
                : string.Empty;

            var log = new ApiLog
            {
                Url = request.RequestUri?.ToString() ?? string.Empty,
                Method = request.Method.Method,
                RequestBody = requestBody,
                CreatedAt = DateTime.Now
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();
                log.ResponseBody = responseBody;
                log.StatusCode = (int)response.StatusCode;
                log.IsSuccess = response.IsSuccessStatusCode;

                if (!response.IsSuccessStatusCode)
                {
                    log.ErrorMessage = $"External API error {response.StatusCode}";
                }

                if (responseBody != null)
                {
                    return JsonSerializer.Deserialize<T>(
                        responseBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }

                return default;
            }
            catch (Exception ex)
            {
                log.IsSuccess = false;
                log.ErrorMessage = ex.ToString();
                log.StatusCode = 0;

                throw;
            }
            finally
            {
                stopwatch.Stop();
                log.Duration = (int)stopwatch.ElapsedMilliseconds;

                await _logService.SaveAsync(log);
            }
        }
    }
}