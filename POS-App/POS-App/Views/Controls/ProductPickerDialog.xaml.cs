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
using System.Windows.Shapes;

namespace POS_App.Views.Controls
{
    /// <summary>
    /// Interaction logic for ProductPickerDialog.xaml
    /// </summary>
    /// 
    public class ProductItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string PriceDisplay => $"{Price:N0}đ";
        public string Sku { get; set; }
        public int CategoryId { get; set; }
        public string Emoji { get; set; }
        public string Badge { get; set; }
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Count { get; set; }
        public string Icon { get; set; }
    }

    public partial class ProductPickerDialog : Window
    {
        private OrderViewModel VM => DataContext as OrderViewModel;

        public ProductPickerDialog()
        {
            InitializeComponent();
        }

        public ProductItem SelectedProduct { get; private set; }

        private readonly List<ProductItem> _allProducts;
        private int _activeCatId = 0;

        public ProductPickerDialog(List<ProductItem> products, List<CategoryItem> categories)
        {
            InitializeComponent();
            _allProducts = products;
            BuildSidebar(categories);
            RenderProducts();
        }

        // ── Build sidebar động giống HTML ──
        private void BuildSidebar(List<CategoryItem> categories)
        {
            bool first = true;
            foreach (var cat in categories)
            {
                var catId = cat.Id;

                // Icon box
                var iconBox = new Border
                {
                    Width = 28,
                    Height = 28,
                    CornerRadius = new CornerRadius(6),
                    Background = first ? new SolidColorBrush(Color.FromRgb(181, 212, 244))
                                       : Brushes.Transparent,
                    Child = new TextBlock
                    {
                        Text = cat.Icon,
                        FontSize = 14,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                // Tên + số lượng
                var nameBlock = new TextBlock
                {
                    Text = cat.Name,
                    FontSize = 12,
                    FontWeight = FontWeights.Medium,
                    Foreground = first ? new SolidColorBrush(Color.FromRgb(24, 95, 165))
                                       : new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                var countBlock = new TextBlock
                {
                    Text = cat.Count,
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150))
                };

                var infoPanel = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
                infoPanel.Children.Add(nameBlock);
                infoPanel.Children.Add(countBlock);

                var row = new StackPanel { Orientation = Orientation.Horizontal };
                row.Children.Add(iconBox);
                row.Children.Add(infoPanel);

                var rb = new RadioButton
                {
                    Style = (Style)FindResource("CatItemStyle"),
                    Content = row,
                    IsChecked = first,
                    Tag = catId
                };

                // Cập nhật màu khi check/uncheck
                rb.Checked += (s, e) =>
                {
                    _activeCatId = catId;
                    nameBlock.Foreground = new SolidColorBrush(Color.FromRgb(24, 95, 165));
                    iconBox.Background = new SolidColorBrush(Color.FromRgb(181, 212, 244));
                    RenderProducts();
                };
                rb.Unchecked += (s, e) =>
                {
                    nameBlock.Foreground = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                    iconBox.Background = Brushes.Transparent;
                };

                CategoryPanel.Children.Add(rb);
                first = false;
            }
        }

        // ── Lọc và hiển thị sản phẩm ──
        private void RenderProducts()
        {
            var list = _activeCatId == 0
                ? _allProducts
                : _allProducts.Where(p => p.CategoryId == _activeCatId).ToList();

            ProductList.ItemsSource = list;
        }

        private void ProductCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b && b.DataContext is ProductItem p)
            {
                VM.AddCart();
                SelectedProduct = p;
                DialogResult = true;
                Close();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
