using System.Text;
using System.Text.Json;
using S3.Gateway.Data;

namespace S3.Gateway.Features.Logs
{
    public class LogBackgroundWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly SemaphoreSlim _fileLock = new(1, 1);

        public LogBackgroundWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var entity in LogQueue.Queue.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var context = scope.ServiceProvider
                        .GetRequiredService<DBContext>();

                    context.Add(entity);

                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    await WriteFileLog(entity, ex);
                }
            }
        }

        private async Task WriteFileLog(object entity, Exception ex)
        {
            try
            {
                string folderPath = @"C:\Logs";
                string filePath = Path.Combine(folderPath, "log-worker-error.log");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var sb = new StringBuilder();

                sb.AppendLine("===== LOG WORKER ERROR =====");
                sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"EntityType: {entity.GetType().Name}");
                sb.AppendLine($"EntityData: {JsonSerializer.Serialize(entity)}");
                sb.AppendLine($"Message: {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    sb.AppendLine("InnerException:");
                    sb.AppendLine(ex.InnerException.ToString());
                }

                sb.AppendLine("============================");
                sb.AppendLine();

                await _fileLock.WaitAsync();

                try
                {
                    await File.AppendAllTextAsync(filePath, sb.ToString());
                }
                finally
                {
                    _fileLock.Release();
                }
            }
            catch
            {
                // tuyệt đối không throw
            }
        }
    }
}