using POS_App.Models.Response;

namespace POS_App.Services.Interfaces
{
    public interface IApiClient
    {
        public interface IApiClient
        {
            Task<T?> PostAsync<T>(
                string url,
                object data)
                where T : ResponseBase, new();

            Task<T?> GetAsync<T>(
                string url)
                where T : ResponseBase, new();
        }
    }
}
