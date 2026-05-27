using POS_App.Dto;
using POS_App.Models;
using POS_App.Models.Request.Auth;
using POS_App.Models.Response.Auth;
using POS_App.Models.Response.Product;
using POS_App.Services;
using POS_App.Services.Api;
using POS_App.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace POS_App.Views
{
    public partial class LoginView : UserControl
    {
        private ObservableCollection<Brush> dots = new();
        private LoginViewModel VM => DataContext as LoginViewModel;

        public LoginView()
        {

            InitializeComponent();

            PasswordDots.ItemsSource = dots;

            InitDots();

            Loaded += (_, __) =>
            {
                PasswordInput.Focus();
            };

        }

        void InitDots()
        {
            dots.Clear();

            for (int i = 0; i < 6; i++)
            {
                dots.Add(
                    new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#D4D4D4")
                    )
                );
            }
        }

        void RefreshDots()
        {
            dots.Clear();

            int length = PasswordInput.Password.Length;

            for (int i = 0; i < 6; i++)
            {
                dots.Add(
                    i < length
                    ? Brushes.Black
                    : new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#D4D4D4")
                    )
                );
            }
        }

        private void PasswordInput_PasswordChanged(
            object sender,
            RoutedEventArgs e)
        {
            RefreshDots();
        }

        private void Number_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (PasswordInput.Password.Length >= 6)
                return;

            var value =
                ((Button)sender)
                .Content
                .ToString();

            PasswordInput.Password += value;

            RefreshDots();

            PasswordInput.Focus();
        }

        private void Backspace_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (PasswordInput.Password.Length > 0)
            {
                PasswordInput.Password =
                    PasswordInput.Password.Substring(
                        0,
                        PasswordInput.Password.Length - 1);
            }

            RefreshDots();

            PasswordInput.Focus();
        }

        private void Clear_Click(
            object sender,
            RoutedEventArgs e)
        {
            PasswordInput.Clear();

            RefreshDots();

            PasswordInput.Focus();
        }

        private async void Enter_Click(
            object sender,
            RoutedEventArgs e)
        {
            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            // Disable keypad tránh bấm liên tục
            this.IsEnabled = false;
            LoadingOverlay.IsEnabled = true;


            var apiClient = new ApiClient();

            var requestData = new LoginRequest
            {
                UserName = "tamla@cfox.vn"
            };

            var response =
                await apiClient.PostAsync<LoginResponse>(
                    "login",
                    requestData);

            this.IsEnabled = true;
            LoadingOverlay.Visibility = Visibility.Collapsed;

            if (response.IsSuccess == false)
            {
                MessageBox.Show(response.Message);
                return;
            }

            var user = new UserDto
            {
                ID = response.YS_UserID,
                Name = response.LastName + " " + response.FirstName,
                POSCode = "P0001",
                Code = response.YS_UserID.ToString(),
                StoreName = response.StoreName,
                YS_AccountID = response.YS_AccountID
            };

            var auth = new AuthSession
            {
                AccessToken = response.Token,
                ExpiresAt = DateTime.Now.AddDays(1000),
                RefreshToken = "",
                SessionId = ""
            };

            SessionService.Instance.Login(
                user,
                auth);

            // vào app luôn
            VM.Login();

            // sync nền
            _ = SyncProductAsync();
        }

        private async Task SyncProductAsync()
        {
            try
            {
                var apiClient =
                    new ApiClient();

                var product =
                    await apiClient
                        .GetAsync<SyncProductResponse>(
                            "product");

                if (!product.IsSuccess)
                {
                    return;
                }

                var items = product.Data.ToList();
                new ProductService().BulkInsert(items);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    ex.ToString());
            }
        }

        private void BtnExitFunction_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}