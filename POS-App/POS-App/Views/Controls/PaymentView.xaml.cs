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

namespace POS_App.Views.Controls
{
    /// <summary>
    /// Interaction logic for PaymentView.xaml
    /// </summary>
    public partial class PaymentView : UserControl
    {
        public PaymentView()
        {
            InitializeComponent();
        }

        private void btnConfirmPayment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PaymentMethod_Click(object sender, RoutedEventArgs e)
        {
            //var allMethods = new[] { btnTienMat, btnVisa, btnQR, btnViDienTu, btnVoucher, btnTachDon };
            //foreach (var btn in allMethods)
            //{
            //    btn.Background = Brushes.White;
            //    btn.BorderBrush = new SolidColorBrush(Color.FromRgb(204, 210, 227));
            //    // Reset text color in children
            //    foreach (var child in ((StackPanel)btn.Content).Children)
            //        if (child is TextBlock tb) tb.Foreground = Brushes.Black;
            //}

            //var clicked = sender as Button;
            //clicked.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96));
            //clicked.BorderBrush = new SolidColorBrush(Color.FromRgb(39, 174, 96));
            //foreach (var child in ((StackPanel)clicked.Content).Children)
            //    if (child is TextBlock tb) tb.Foreground = Brushes.White;
        }
    }
}
