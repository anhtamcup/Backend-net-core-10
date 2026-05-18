using POS_App.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace POS_App.Views
{
    public partial class LoginView : UserControl
    {
        private LoginViewModel VM => DataContext as LoginViewModel;
        private readonly MainViewModel _main;

        // Password thật
        private string _password = "";

        // Danh sách chấm hiển thị
        private ObservableCollection<string> _dots =
            new ObservableCollection<string>();

        public LoginView()
        {
            InitializeComponent();

            InitPasswordDisplay();

            PasswordDots.ItemsSource = _dots;
        }

        private void InitPasswordDisplay()
        {
            _dots.Clear();

            // Luôn hiển thị 6 ô
            for (int i = 0; i < 6; i++)
            {
                _dots.Add("○");
            }
        }

        private void UpdatePasswordDisplay()
        {
            for (int i = 0; i < 6; i++)
            {
                _dots[i] =
                    i < _password.Length
                        ? "●"
                        : "○";
            }

            // ép refresh UI
            PasswordDots.Items.Refresh();
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                // giới hạn 6 số
                if (_password.Length >= 6)
                    return;

                _password += btn.Content.ToString();

                UpdatePasswordDisplay();
            }
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_password))
            {
                _password = _password.Substring(
                    0,
                    _password.Length - 1);

                UpdatePasswordDisplay();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _password = "";

            UpdatePasswordDisplay();
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                $"Password: {_password}",
                "Login");

            // ví dụ:
            // VM.LoginCommand.Execute(_password);
        }
    }
}