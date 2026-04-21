namespace S3.Gateway.Integrations.Base
{
    public interface IBaseApiClient
    {
        Task<TResponse?> GetAsync<TResponse>(string url, Dictionary<string, string>? headers = null);

        Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request, Dictionary<string, string>? headers = null);

        Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request, Dictionary<string, string>? headers = null);
    }
}
