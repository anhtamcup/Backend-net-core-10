using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace POS_App.Views.Controls
{
    public partial class ToastWindow : Window
    {
        public ToastWindow(string message)
        {
            InitializeComponent();

            txtMessage.Text = message;

            Loaded += Toast_Loaded;
        }

        private async void Toast_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            var main = Application.Current.MainWindow;

            if (main != null)
            {
                // góc phải trên của MainWindow
                Left = main.Left + main.ActualWidth - Width - 20;

                Top = main.Top + 20;

                // để toast đi theo main
                Owner = main;
            }

            Opacity = 0;

            var fadeIn = new DoubleAnimation(
                0,
                1,
                TimeSpan.FromMilliseconds(250));

            BeginAnimation(
                OpacityProperty,
                fadeIn);

            await Task.Delay(3000);

            var fadeOut = new DoubleAnimation(
                1,
                0,
                TimeSpan.FromMilliseconds(400));

            fadeOut.Completed += (_, __) => Close();

            BeginAnimation(
                OpacityProperty,
                fadeOut);
        }
    }
}