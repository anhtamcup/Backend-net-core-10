using POS_App.Dto;
using POS_App.ViewModels;
using POS_App.Views.Controls;
using System.Windows;
using System.Windows.Controls;

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
            var categories = new List<CategoryItem>
            {
                new() { Id=0, Name="Tất cả SP",    Count="56 mặt hàng", Icon="🛒" },
                new() { Id=1, Name="Tươi sống",    Count="7 mặt hàng",  Icon="🥬" },
                new() { Id=2, Name="Đồ uống",      Count="9 mặt hàng",  Icon="🥤" },
                new() { Id=3, Name="Bánh kẹo",     Count="7 mặt hàng",  Icon="🍪" },
                new() { Id=4, Name="Mì-Phở",       Count="6 mặt hàng",  Icon="🍜" },
                new() { Id=5, Name="Gia vị",       Count="7 mặt hàng",  Icon="🧂" },
                new() { Id=6, Name="Sữa các loại", Count="7 mặt hàng",  Icon="🥛" },
                new() { Id=7, Name="Hoá mỹ phẩm",  Count="7 mặt hàng",  Icon="💄" },
                new() { Id=8, Name="Đồ gia dụng",  Count="6 mặt hàng",  Icon="🧹" },
            };

            var products = new List<ProductItem>
            {
                new() { Name="Trứng gà Ba Huân vỉ 10",      Price=36000,  Sku="8934567000011·vỉ",  CategoryId=1, Emoji="🥚", Badge="48 vỉ" },
                new() { Name="Cá basa fillet đông lạnh 500g", Price=79000,  Sku="8934567000028·gói", CategoryId=1, Emoji="🐟", Badge="14 gói" },
                new() { Name="Thịt heo ba chỉ tươi 500g",    Price=118000, Sku="8934567000035·kg",  CategoryId=1, Emoji="🥩", Badge="22 kg" },
                new() { Name="Coca-Cola lon 330ml",           Price=12000,  Sku="8934588063015·lon", CategoryId=2, Emoji="🥤", Badge="248 lon" },
                new() { Name="Trà xanh không độ 500ml",       Price=11000,  Sku="8934588063046·chai",CategoryId=2, Emoji="🍵", Badge="120 chai" },
                // thêm tiếp...
            };

            var dialog = new ProductPickerDialog(products, categories);
            if (dialog.ShowDialog() == true)
            {
                var p = dialog.SelectedProduct;
                // xử lý p.Name, p.Price, ...
            }
            //VM?.AddCart();
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
    }
}