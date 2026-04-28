using Microsoft.Extensions.Caching.Memory;

namespace S3.Gateway.Integrations.Ekyc.Napas
{
    public class NapasTokenService
    {
        private readonly IMemoryCache _cache;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private const string TOKEN_CACHE_KEY = "NAPAS_ACCESS_TOKEN";

        public NapasTokenService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<string> GetToken(Func<Task<NpOAuthResponse>> factory)
        {
            if (_cache.TryGetValue(TOKEN_CACHE_KEY, out string token))
                return token;

            await _lock.WaitAsync();

            try
            {
                if (_cache.TryGetValue(TOKEN_CACHE_KEY, out token))
                    return token;

                var response = await factory();

                var expire = Math.Max(int.Parse(response.ExpiresIn) - 30, 1);

                _cache.Set(
                    TOKEN_CACHE_KEY,
                    response.AccessToken,
                    TimeSpan.FromSeconds(expire));

                return response.AccessToken;
            }
            finally
            {
                _lock.Release();
            }
        }

        public void ClearToken()
        {
            _cache.Remove(TOKEN_CACHE_KEY);
        }
    }
}
