using POS_App.Helpers;
using POS_App.ViewModels;
using POS_App.Views;
using System.Windows;
using System.Windows.Media;
using WpfScreenHelper;

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
            var mainVm = new MainViewModel();
            DataContext = mainVm;
            //OpenCustomerScreen(mainVm);
            //RequestFullScreen();
        }

        private void RequestFullScreen()
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Normal; // Reset trước để tránh lỗi taskbar
            WindowState = WindowState.Maximized;
        }

        private void btnLog_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm?.Logout();
        }

        // Ở MainWindow hoặc nơi bạn mở CustomerWindow
        private CustomerWindow _customerWindow;
        private void OpenCustomerScreen(MainViewModel mainVm)
        {
            var secondScreen = Screen.AllScreens.FirstOrDefault(s => !s.Primary);
            if (secondScreen == null) return;

            _customerWindow = new CustomerWindow(mainVm);
            _customerWindow.WindowStyle = WindowStyle.None;
            _customerWindow.ResizeMode = ResizeMode.NoResize;
            _customerWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            _customerWindow.Left = secondScreen.WpfBounds.Left;
            _customerWindow.Top = secondScreen.WpfBounds.Top;
            _customerWindow.Width = secondScreen.WpfBounds.Width;
            _customerWindow.Height = secondScreen.WpfBounds.Height;
            _customerWindow.Show();

            // Delay để WPF render xong vị trí rồi mới Maximize
            _customerWindow.Dispatcher.InvokeAsync(() =>
            {
                _customerWindow.WindowState = WindowState.Maximized;
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

    }
}