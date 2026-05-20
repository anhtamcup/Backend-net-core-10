using POS_App.ViewModels;
using System.Collections.ObjectModel;
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

        private void Enter_Click(
            object sender,
            RoutedEventArgs e)
        {
            PasswordInput.Focus();
            if (PasswordInput.Password == "000000")
            {
                VM.Login();
            }
            else
            {
                MessageBox.Show("Sai thông tin tài khoản hoặc mật khẩu");
            }
        }

        private void BtnExitFunction_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}