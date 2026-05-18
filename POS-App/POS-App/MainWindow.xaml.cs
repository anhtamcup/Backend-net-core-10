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
            OpenCustomerScreen(mainVm);
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
            _customerWindow.Left = secondScreen.WorkingArea.Left;
            _customerWindow.Top = secondScreen.WorkingArea.Top;
            _customerWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            _customerWindow.WindowState = WindowState.Maximized;
            _customerWindow.Show();
        }

    }
}