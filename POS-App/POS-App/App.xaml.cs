using POS_App.Services;
using System.Windows;

namespace POS_App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // App.xaml.cs
        public static DatabaseService Database { get; } = new DatabaseService();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //if (SessionService.Instance.TryRestore())
            //    new MainWindow().Show();
            //else
            //    new LoginWindow().Show();
        }
    }

}
