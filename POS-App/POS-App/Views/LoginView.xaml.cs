using POS_App.Dto;
using POS_App.Helpers;
using POS_App.Models;
using POS_App.Models.Request.Auth;
using POS_App.Models.Response.Auth;
using POS_App.Models.Response.Data;
using POS_App.Models.Response.Product;
using POS_App.Services;
using POS_App.Services.Api;
using POS_App.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using static POS_App.Models.Response.Auth.LoginResponse;

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

        private void BtnConfirmBranch_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (lbBranches.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn chi nhánh");
                return;
            }

            var branch = (BranchAccess)lbBranches.SelectedItem;
            var user = new UserDto
            {
                ID = _loginResponse.YS_UserID,
                Name = _loginResponse.LastName + " " + _loginResponse.FirstName,
                POSCode = branch.YS_BranchID.ToString(),
                Code = _loginResponse.YS_UserID.ToString(),
                StoreName = _loginResponse.StoreName,
                YS_AccountID = _loginResponse.YS_AccountID,
                BranchID = branch.YS_BranchID
            };

            var auth = new AuthSession
            {
                AccessToken = _loginResponse.Token,
                ExpiresAt = DateTime.Now.AddDays(1000),
                RefreshToken = "",
                SessionId = ""
            };

            SessionService.Instance.Login(user, auth);

            // vào app luôn
            VM.Login();
            _ = SyncDataAsync();
        }

        private LoginResponse _loginResponse;
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

                if (string.IsNullOrWhiteSpace(staffCode) || string.IsNullOrWhiteSpace(pass))
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

                _loginResponse = response;
                lbBranches.ItemsSource = response.Branchs;
                BranchOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {

            }
            finally
            {

                this.IsEnabled = true;
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }

        }

        private async Task SyncDataAsync()
        {
            try
            {
                var apiClient = new ApiClient();
                var dataResponse = await apiClient.GetAsync<SyncDataResponse>("data");

                if (dataResponse == null || dataResponse.IsSuccess == false)
                {
                    MessageBox.Show("Đồng bộ dữ liệu thất bại, vui lòng thử lại");
                    return;
                }

                var products = dataResponse.Products.ToList();
                var payments = dataResponse.Payments.ToList();
                var db = new DatabaseService();
                new ProductService(db).BulkInsert(products);
                new PaymentService(db).BulkInsert(payments);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lưu dữ liệu đồng bộ thất bại, vui lòng thử lại");
                return;
            }
        }

        private void BtnExitFunction_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}