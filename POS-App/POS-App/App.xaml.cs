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
    }

}
