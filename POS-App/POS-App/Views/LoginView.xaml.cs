using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POS_App.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void txt1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txt1.Text.Length == 1)
            {
                txt2.Focus();
            }
        }

        private void txt2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txt2.Text.Length == 1)
            {
                txt3.Focus();
            }
        }

        private void txt3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txt3.Text.Length == 1)
            {
                txt4.Focus();
            }
        }

        private void txt4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt1.Text) == false &&
                string.IsNullOrWhiteSpace(txt2.Text) == false &&
                string.IsNullOrWhiteSpace(txt3.Text) == false &&
                string.IsNullOrWhiteSpace(txt4.Text) == false
                )
            {
                Button_Click(sender, e);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string pin =
                txt1.Text +
                txt2.Text +
                txt3.Text +
                txt4.Text;

            if (pin == "0000")
            {
                MainWindow main =
                    (MainWindow)Application.Current.MainWindow;

                //main.MainContent.Content = new OrderView();
            }
            else
            {
                txtError.Text = "Sai mã PIN";
                txtError.Visibility = Visibility.Visible;
            }
        }
    }
}
