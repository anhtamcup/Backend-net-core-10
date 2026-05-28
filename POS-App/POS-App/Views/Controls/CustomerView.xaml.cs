using POS_App.Models.Request.Customer;
using POS_App.Models.Response.Customer;
using POS_App.Services.Api;
using POS_App.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace POS_App.Views.Controls
{
    /// <summary>
    /// Interaction logic for CustomerView.xaml
    /// </summary>
    public partial class CustomerView : UserControl
    {
        public ObservableCollection<CustomerInfor> Customers { get; set; } = new ObservableCollection<CustomerInfor>();
        OrderViewModel _orderVM;
        public CustomerView(OrderViewModel orderVM)
        {
            InitializeComponent();

            _orderVM = orderVM;

            DataContext = this;

            _ = SearchCustomers();
        }

        private async Task SearchCustomers()
        {
            try
            {
                var apiClient = new ApiClient();

                var requestData = new SearchCustomerRequest
                {
                    KeySearch = tbSearch.Text.Trim(),
                    PageIndex = 1,
                    PageSize = 10
                };

                var response = await apiClient.PostAsync<SearchCustomerResponse>(
                    "Customer",
                    requestData);

                if (response.IsSuccess == false)
                {
                    MessageBox.Show(response.Message);
                    return;
                }

                // clear dữ liệu cũ
                Customers.Clear();

                // add dữ liệu mới
                foreach (var item in response.Customers)
                {
                    Customers.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void tbSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _ = SearchCustomers();
            }
        }

        private void listboxCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            var selectedCustomer = listBox.SelectedItem as CustomerInfor;

            if (selectedCustomer == null) return;

            _orderVM.SetCustomer(new CustomerInfoViewModel
            {
                ID = selectedCustomer.YS_CustomerID,
                Name = selectedCustomer.CustomerName,
                Phone = selectedCustomer.Phone,
                Point = selectedCustomer.LoyaltyPoint,
                MemberRank = selectedCustomer.MemberRank
            });

            Window.GetWindow(this)?.Close();
        }
    }
}
