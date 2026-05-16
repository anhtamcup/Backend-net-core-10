using POS_App.Helpers;
using POS_App.ViewModels;
using POS_App.Views;
using System.Windows;
using System.Windows.Media;
//using System.Windows.Forms;
namespace POS_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            InitializeComponent();
            DataContext = new MainViewModel();

            Loaded += MainWindow_Loaded;
            OpenCustomerScreen();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;

            var area = SystemParameters.WorkArea;

            Left = area.Left;
            Top = area.Top;
            Width = area.Width;
            Height = area.Height;

            Topmost = true;
        }

        private void btnLog_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm?.Logout();
        }

        private void OpenCustomerScreen()
        {
            var rect = MonitorUtil.GetSecondaryScreen(this);

            var w = new CustomerWindow();

            w.WindowStartupLocation = WindowStartupLocation.Manual;
            w.Left = rect.Left;
            w.Top = rect.Top;
            w.WindowState = WindowState.Maximized;

            w.Show();
        }
    }
}