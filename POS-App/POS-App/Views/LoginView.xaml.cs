using POS_App.Dto;
using POS_App.Helpers;
using POS_App.Models;
using POS_App.Models.Request.Auth;
using POS_App.Models.Response.Auth;
using POS_App.Models.Response.Product;
using POS_App.Services;
using POS_App.Services.Api;
using POS_App.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace POS_App.Views
{
    public partial class LoginView : UserControl
    {
        private LoginViewModel VM => DataContext as LoginViewModel;

        public LoginView()
        {
            InitializeComponent();
            tbStaffCode.Focus();
        }

        private void Number_Click(
            object sender,
            RoutedEventArgs e)
        {
            KeypadHelper.Insert((sender as Button)?.Content?.ToString());
        }

        private void Backspace_Click(
            object sender,
            RoutedEventArgs e)
        {
            KeypadHelper.Backspace();
        }

        private void Clear_Click(
            object sender,
            RoutedEventArgs e)
        {
            KeypadHelper.Clear();
        }

        private async void Enter_Click(
            object sender,
            RoutedEventArgs e)
        {
            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                this.IsEnabled = false;
                LoadingOverlay.IsEnabled = true;

                var apiClient = new ApiClient();
                var staffCode = tbStaffCode.Text;
                var pass = tbPassword.Password;

                if(string.IsNullOrWhiteSpace(staffCode) || string.IsNullOrWhiteSpace(pass))
                {
                    ToastService.Error("Mã nhân viên hoặc mật khẩu không được bỏ trống");
                    return;
                }

                var requestData = new LoginRequest
                {
                    UserName = staffCode,
                    Password = pass
                };

                var response =
                    await apiClient.PostAsync<LoginResponse>(
                        "login",
                        requestData);

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

                SessionService.Instance.Login(user, auth);

                // vào app luôn
                VM.Login();

                // sync nền
                _ = SyncProductAsync();
            }
            catch(Exception ex)
            {

            }
            finally
            {

                this.IsEnabled = true;
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }

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