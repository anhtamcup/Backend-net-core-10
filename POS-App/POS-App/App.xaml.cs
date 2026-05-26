using Microsoft.Extensions.Configuration;
using Notification.Wpf;
using POS_App.Config;
using POS_App.Services;
using System;
using System.Windows;

namespace POS_App
{
    public partial class App : Application
    {
        public static NotificationManager NotificationManager { get; } = new NotificationManager();
        public static DatabaseService Database { get; }
            = new DatabaseService();

        public static AppConfig Config { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false,
                    reloadOnChange: true)
                .Build();

            Config = configuration.Get<AppConfig>();
        }
    }
}