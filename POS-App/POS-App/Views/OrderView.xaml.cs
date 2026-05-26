using POS_App.Dto;
using POS_App.Helpers;
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
            VM.SetCustomer(new CustomerInfoViewModel
            {
                Name = "Cao Hùng",
                Phone = "0338162614",
                Point = 1000
            });
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