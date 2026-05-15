using POS_App.Dto;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace POS_App.Views
{
    public partial class OrderView : UserControl
    {
        private ObservableCollection<CartItemRow> cartItems;
        private CollectionViewSource cvs;

        public OrderView()
        {
            InitializeComponent();
            cartItems = new ObservableCollection<CartItemRow>();

            //for (int i = 0; i < 5; i++)
            //{
            //    cartItems.Add(new CartItemRow
            //    {
            //        ID = i,
            //        Index = i + 1,
            //        Code = "0000000" + i,
            //        Name = "Nước suối Lavie 500ml",
            //        OriginalPrice = 10000,
            //        Quantity = 2,
            //        Unit = "Chai",
            //        DiscountPrice = 0
            //    });
            //}

            cvs = new CollectionViewSource();
            cvs.Source = cartItems;

            cvs.SortDescriptions.Add(
                new System.ComponentModel.SortDescription(
                    nameof(CartItemRow.Index),
                    System.ComponentModel.ListSortDirection.Descending));

            grCartItems.ItemsSource = cvs.View;
            Calculator();
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CartItemRow;

            if (item == null)
                return;

            item.Quantity++;
            Calculator();
        }

        private void btnMinus_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CartItemRow;

            if (item == null)
                return;

            item.Quantity--;

            if (item.Quantity <= 0)
            {
                cartItems.Remove(item);
            }
            Calculator();
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CartItemRow;

            if (item == null)
                return;

            cartItems.Remove(item);
            Calculator();
        }

        private void Calculator()
        {
            var totalItem = cartItems.Sum(item => item.Quantity);
            var discountPrice = cartItems.Sum(item => item.DiscountPrice);
            var subTotalAmount = cartItems.Sum(item => item.Quantity * item.OriginalPrice);
            var totalAmount = cartItems.Sum(item => item.TotalPrice);
            var vatAmount = cartItems.Sum(item => item.VATPrice);
            var cartSummary = new CartSummary
            {
                TotalItem = totalItem,
                PromotionAmount = discountPrice,
                SubTotalAmount = subTotalAmount,
                TotalAmount = totalAmount,
                VATAmount = vatAmount
            };

            summaryTotalItem.Text = cartSummary.TotalItem.ToString();
            summarySubTotalAmount.Text = cartSummary.SubTotalAmount.ToString("N0") + " đ";
            summaryPromotionAmount.Text = cartSummary.PromotionAmount.ToString("N0") + " đ";
            summaryVATAmount.Text = cartSummary.VATAmount.ToString("N0") + " đ";
            summaryTotalAmount.Text = cartSummary.TotalAmount.ToString("N0") + " đ";
        }

        private void btnSearchProduct_Click(object sender, RoutedEventArgs e)
        {
            var totalCartItem = cartItems.Count;
            cartItems.Add(new CartItemRow
            {
                ID = 1000,
                Index = totalCartItem + 1,
                Code = "0000000",
                Name = "Nước suối Lavie 500ml",
                OriginalPrice = 10000,
                Quantity = 1,
                Unit = "Chai",
                DiscountPrice = 0
            });

            Calculator();
        }
    }
}