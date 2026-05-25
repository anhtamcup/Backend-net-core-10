using Microsoft.Extensions.Configuration;

namespace POS_App.Config
{
    public static class ConfigManager
    {
        public static AppConfig Settings { get; }

        static ConfigManager()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .Build();

            Settings = configuration
                .Get<AppConfig>()!;
        }
    }
}
