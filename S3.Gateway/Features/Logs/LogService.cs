using System.Threading.Channels;

namespace S3.Gateway.Features.Logs
{
    public static class LogQueue
    {
        public static readonly Channel<object> Queue =
            Channel.CreateUnbounded<object>();
    }

    public class LogService : ILogService
    {
        public async Task SaveAsync<T>(T entity) where T : class
        {
            await LogQueue.Queue.Writer.WriteAsync(entity);
        }
    }
}
