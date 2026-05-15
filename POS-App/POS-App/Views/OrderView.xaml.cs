using POS_App.Dto;
using POS_App.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace POS_App.Views
{
    public partial class OrderView : UserControl
    {
        private OrderViewModel VM =>
            (OrderViewModel)DataContext;

        public OrderView()
        {
            InitializeComponent();

            DataContext = new OrderViewModel();
        }

        private void btnPlus_Click(
            object sender,
            RoutedEventArgs e)
        {
            var item =
            (sender as Button)?
            .DataContext as CartItemRow;

            if (item == null)
                return;

            VM.Increase(item);
        }

        private void btnMinus_Click(
            object sender,
            RoutedEventArgs e)
        {
            var item =
            (sender as Button)?
            .DataContext as CartItemRow;

            if (item == null)
                return;

            VM.Decrease(item);
        }

        private void btnRemoveItem_Click(
            object sender,
            RoutedEventArgs e)
        {
            var item =
            (sender as Button)?
            .DataContext as CartItemRow;

            if (item == null)
                return;

            VM.Remove(item);
        }

        private void btnSearchProduct_Click(
            object sender,
            RoutedEventArgs e)
        {
            VM.AddCart();
        }
    }
}