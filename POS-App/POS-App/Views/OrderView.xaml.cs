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

            for (int i = 0; i < 10; i++)
            {
                cartItems.Add(new CartItemRow
                {
                    ID = i,
                    Index = i + 1,
                    Code = "0000000" + i,
                    Name = "Nước suối Lavie 500ml",
                    OriginalPrice = 10000,
                    Quantity = 2,
                    Unit = "Chai",
                    DiscountPrice = 0
                });
            }

            cvs = new CollectionViewSource();
            cvs.Source = cartItems;

            cvs.SortDescriptions.Add(
                new System.ComponentModel.SortDescription(
                    nameof(CartItemRow.Index),
                    System.ComponentModel.ListSortDirection.Descending));

            grCartItems.ItemsSource = cvs.View;
        }

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CartItemRow;

            if (item == null)
                return;

            item.Quantity++;
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
        }

        private void btnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.DataContext as CartItemRow;

            if (item == null)
                return;

            cartItems.Remove(item);
        }
    }
}