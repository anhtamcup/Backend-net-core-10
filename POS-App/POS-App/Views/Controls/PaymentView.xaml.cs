using POS_App.ViewModels;
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
        public class Item
        {
            public string Text { get; set; }

            public string Icon { get; set; }
            private string Path = "/Assets/Icons/Payment";

            public string IconPath { get { return string.Format("{0}/{1}.svg", Path, Icon); } }
            public string IconPathSelected { get { return string.Format("{0}/{1}-2.svg", Path, Icon); } }
        }

        public PaymentView(OrderViewModel orderVM)
        {
            InitializeComponent();
            DataContext = orderVM;

            var paymentList = new List<Item>()
            {
                new Item { Text = "Tiền mặt", Icon="ico-cash" },
                new Item { Text = "VISA",  Icon="ico-visa" },
                new Item { Text = "QR",  Icon="ico-qrcode" },
                new Item { Text = "Ví điện tử",  Icon="ico-wallet" },
                new Item { Text = "Voucher", Icon="ico-voucher" },
                new Item { Text = "Tách đơn",  Icon="ico-desk" },
            };

            PaymentList.ItemsSource = paymentList;
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
