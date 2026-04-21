using S3.Gateway.Entities;

namespace S3.Gateway.Features.Logs
{
    public interface ILogService
    {
        Task SaveAsync<T>(T entity) where T : class;
    }
}
