using System.Windows;
using System.Windows.Controls;

namespace POS_App.Views.Controls
{
    /// <summary>
    /// Interaction logic for CashView.xaml
    /// </summary>
    public partial class CashView : UserControl
    {
        public CashView()
        {
            InitializeComponent();

            txtCustomerAmount.Focus();
            Loaded += CashView_Loaded;
        }

        private void CashView_Loaded(object sender, RoutedEventArgs e)
        {
            txtCustomerAmount.Focus();
        }
    }
}
