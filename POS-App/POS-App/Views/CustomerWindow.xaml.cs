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

namespace POS_App.Views
{
    /// <summary>
    /// Interaction logic for CustomerWindow.xaml
    /// </summary>
    public partial class CustomerWindow : Window
    {
        public CustomerWindow(MainViewModel mainVm)
        {
            InitializeComponent();
            DataContext = mainVm;

            // Lắng nghe khi CurrentView thay đổi
            mainVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.CurrentView))
                    UpdateUI();
            };

            Loaded += (s, e) => UpdateUI();
        }

        private void UpdateUI()
        {
            //var vm = DataContext as MainViewModel;
            //OrderPanel.Visibility = vm?.CurrentView is OrderViewModel
            //    ? Visibility.Visible
            //    : Visibility.Collapsed;
        }
    }
}
