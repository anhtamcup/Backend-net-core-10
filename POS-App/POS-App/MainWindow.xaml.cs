using POS_App.Dto;
using POS_App.Services;
using POS_App.ViewModels;
using POS_App.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            if (SessionService.Instance.TryRestore())
            {
                mainVm.CurrentView = new OrderViewModel();
            }

            DataContext = mainVm;
            KeyDown += MainWindow_KeyDown;


            //OpenCustomerScreen(mainVm);
            //ToggleFullScreen();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F11)
            {
                ToggleFullScreen();
                e.Handled = true;
            }
        }

        private void ToggleFullScreen()
        {
            bool isFullScreen = WindowStyle == WindowStyle.None
                             && WindowState == WindowState.Maximized;

            if (isFullScreen)
            {
                // Thoát fullscreen
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                // Vào fullscreen
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Normal;  // Reset trước để tránh lỗi taskbar
                WindowState = WindowState.Maximized;
            }
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

        private void tbSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            var txt = (TextBox)sender;
            var barcode = txt.Text.Trim();
            if (string.IsNullOrEmpty(barcode)) return;

            var productService = new ProductService();
            var product = productService.GetByBarcode(barcode);

            if (product != null)
            {
                var main = DataContext as MainViewModel;
                var orderViewModel = main.CurrentView as OrderViewModel;

                var cart = new CartItemRow
                {
                    ID = product.YS_ProductID,
                    Code = product.ProductCode,
                    Name = product.ProductName,
                    Quantity = 1,
                    Unit = "Chai",
                    OriginalPrice = product.RetailPrice ?? 0m
                };

                orderViewModel.AddCart(cart);
            }
            else
                MessageBox.Show("Không tìm thấy sản phẩm!");

            txt.Clear();
            txt.Focus();
            e.Handled = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            _customerWindow?.Close();
            base.OnClosed(e);
        }
    }
}