using POS_App.Dto;
using POS_App.Helpers;
using POS_App.Services;
using POS_App.ViewModels;
using POS_App.Views.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS_App.Views
{
    public partial class OrderView : UserControl
    {
        private OrderViewModel VM => DataContext as OrderViewModel;

        public OrderView()
        {
            InitializeComponent();
        }

        private void btnPlus_Click(
            object sender,
            RoutedEventArgs e)
        {
            var item =
                (sender as Button)?.DataContext as CartItemRow;

            if (item == null || VM == null)
                return;

            VM.Increase(item);
        }

        private void btnMinus_Click(
            object sender,
            RoutedEventArgs e)
        {
            var item =
                (sender as Button)?.DataContext as CartItemRow;

            if (item == null || VM == null)
                return;

            VM.Decrease(item);
        }

        private void btnRemoveItem_Click(
            object sender,
            RoutedEventArgs e)
        {
            var item =
                (sender as Button)?.DataContext as CartItemRow;

            if (item == null || VM == null)
                return;

            VM.Remove(item);
        }

        private void btnSearchProduct_Click(
            object sender,
            RoutedEventArgs e)
        {
            var productService = new ProductService();
            var product = productService.GetByBarcode("P0001748160");

            if (product != null)
            {
                var cart = new CartItemRow
                {
                    ID = product.YS_ProductID,
                    Code = product.ProductCode,
                    Name = product.ProductName,
                    Quantity = 1,
                    Unit = "Chai",
                    OriginalPrice = product.RetailPrice ?? 0m
                };

                VM.AddCart(cart);
            }
        }

        private void tbQuantity_PreviewTextInput(
            object sender,
            System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void tbQuantity_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            VM?.Recalculate();
        }

        private void btnCustomerFunction_Click(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);

            if (owner == null)
                return;

            var modal = new AppModal
            {
                Owner = owner,
                // ❌ Bỏ dòng này: WindowStartupLocation = WindowStartupLocation.Manual,

                Width = owner.ActualWidth,
                Height = owner.ActualHeight,

                Left = owner.Left,
                Top = owner.Top,

                ModalTitle = "Khách hàng thân thiết",
                ModalWidth = 400,
                ModalHeight = 500,
                ModalContent = new CustomerView(VM)
            };

            modal.ShowDialog();
        }

        private void btnPayment_Click(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);

            if (owner == null)
                return;

            var modal = new AppModal
            {
                Owner = owner,
                // ❌ Bỏ dòng này: WindowStartupLocation = WindowStartupLocation.Manual,

                Width = owner.ActualWidth,
                Height = owner.ActualHeight,

                Left = owner.Left,
                Top = owner.Top,

                ModalTitle = "Thanh toán hóa đơn",
                ModalWidth = 800,
                ModalHeight = 500,
                ModalContent = new PaymentView(VM)
            };

            modal.ShowDialog();
        }


        private void Number_Click(object sender, RoutedEventArgs e)
        {
            KeypadHelper.Insert(
                (sender as Button)?.Content?.ToString());
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            KeypadHelper.Backspace();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            KeypadHelper.Clear();
        }
    }
}