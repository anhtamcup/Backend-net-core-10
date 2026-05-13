
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
            //CreateKeyboard();
        }

        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string value = btn.Content.ToString();

            MessageBox.Show(value);
        }

        private void AddButton(Grid grid, string text, int row, int col, string backgroundColor = "", bool isEnter = false)
        {
            Button btn = new Button
            {
                Height=54,
                Width=109,
                Content = text,
                Margin = new Thickness(1),
                FontSize = 18,
                BorderThickness = new Thickness(0)
            };

            btn.Background = (Brush)FindResource(backgroundColor);
            btn.Foreground = (Brush)FindResource("keyboard-button-text");

            Grid.SetRow(btn, row);
            Grid.SetColumn(btn, col);
            Grid.SetRowSpan(btn, isEnter ? 2 : 1);

            grid.Children.Add(btn);
        }

        private void CreateKeyboard()
        {
            Grid grid = new Grid();

            // rows
            for (int i = 0; i < 5; i++)
                grid.RowDefinitions.Add(new RowDefinition());

            // columns
            for (int i = 0; i < 4; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());

            // Row 0
            AddButton(grid, "7", 0, 0, backgroundColor: "bg-button-white");
            AddButton(grid, "8", 0, 1, backgroundColor: "bg-button-white");
            AddButton(grid, "9", 0, 2, backgroundColor: "bg-button-white");
            AddButton(grid, "⌫", 0, 3, backgroundColor: "bg-button-gray");

            // Row 1
            AddButton(grid, "4", 1, 0, backgroundColor: "bg-button-white");
            AddButton(grid, "5", 1, 1, backgroundColor: "bg-button-white");
            AddButton(grid, "6", 1, 2, backgroundColor: "bg-button-white");
            AddButton(grid, "↑", 1, 3, backgroundColor: "bg-button-gray");

            // Row 2
            AddButton(grid, "1", 2, 0, backgroundColor: "bg-button-white");
            AddButton(grid, "2", 2, 1, backgroundColor: "bg-button-white");
            AddButton(grid, "3", 2, 2, backgroundColor: "bg-button-white");
            AddButton(grid, "↓", 2, 3, backgroundColor: "bg-button-gray");

            // Row 3
            AddButton(grid, "%", 3, 0, backgroundColor: "bg-button-white");
            AddButton(grid, ",", 3, 1, backgroundColor: "bg-button-white");
            AddButton(grid, "0", 3, 2, backgroundColor: "bg-button-white");

            // ENTER span 2 rows
            AddButton(grid, "ENTER", 3, 3, backgroundColor: "bg-button-green", isEnter: true);

            // Row 4
            AddButton(grid, "CANCEL", 4, 0, backgroundColor: "bg-button-gray");
            AddButton(grid, "DEL", 4, 1, backgroundColor: "bg-button-gray");
            AddButton(grid, "CLEAR", 4, 2, backgroundColor: "bg-button-gray");

            //KeyboardContainer.Children.Add(grid);
        }
    }
}
