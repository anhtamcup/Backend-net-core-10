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

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            var textBox = Keyboard.FocusedElement as TextBox;

            if (textBox == null)
                return;

            var value = (sender as Button)?.Content?.ToString();

            textBox.Text += value;
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            var textBox = Keyboard.FocusedElement as TextBox;

            if (textBox == null)
                return;

            if (!string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = textBox.Text[..^1];
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            (Keyboard.FocusedElement as TextBox)?.Clear();
        }

        private void Money_Click(object sender, RoutedEventArgs e)
        {
            var textBox = Keyboard.FocusedElement as TextBox;
            if (textBox == null)
                return;

            var btn = sender as Button;

            var text = btn?.Content?.ToString();

            if (text == null)
                return;

            int amount = text switch
            {
                "10k" => 10000,
                "20k" => 20000,
                "50k" => 50000,
                "100k" => 100000,
                "200k" => 200000,
                "500k" => 500000,
                "1000k" => 1000000,
                "2000k" => 2000000,
                _ => 0
            };

            textBox.Text = amount.ToString("N0");
        }
    }
}
