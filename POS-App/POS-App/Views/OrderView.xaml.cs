
using POS_App.Dto;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace POS_App.Views
{
    /// <summary>
    /// Interaction logic for OrderView.xaml
    /// </summary>
    public partial class OrderView : UserControl
    {
        public OrderView()
        {
            InitializeComponent();


            var cartItems = new List<CartItemRow>();
            cartItems.Add(new CartItemRow
            {
                ID = 0,
                Index = 1,
                Code = "00000001",
                Name = "Nước suôits Lavie 500ml",
                OriginalPrice = 10000,
                Quantity = 2,
                Unit = "Chai",
                DiscountPrice = 0,
                TotalPrice = 20000
            });

            cartItems.Add(new CartItemRow
            {
                ID = 0,
                Index = 1,
                Code = "00000002",
                Name = "Nước suôits Lavie 500ml",
                OriginalPrice = 100000,
                Quantity = 2,
                Unit = "Chai",
                DiscountPrice = 0,
                TotalPrice = 200000
            });

            grCartItems.ItemsSource = cartItems;
        }
    }
}
