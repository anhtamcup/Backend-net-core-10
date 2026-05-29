using Microsoft.Windows.Themes;
using Notification.Core;
using Notification.Wpf;
using POS_App.Helpers;
using POS_App.Models;
using POS_App.Models.Request.Invoice;
using POS_App.Models.Response.Invoice;
using POS_App.Services;
using POS_App.Services.Api;
using POS_App.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static POS_App.Views.Controls.PaymentView;
using static POS_App.Views.Controls.PaymentView.PaymentGroup;

namespace POS_App.Views.Controls
{
    /// <summary>
    /// Interaction logic for PaymentView.xaml
    /// </summary>
    public partial class PaymentView : UserControl
    {
        public class Item
        {
            public string Text { get; set; }

            public string Icon { get; set; }
            private string Path = "/Assets/Icons/Payment";

            public string IconPath { get { return string.Format("{0}/{1}.svg", Path, Icon); } }
            public string IconPathSelected { get { return string.Format("{0}/{1}-2.svg", Path, Icon); } }
        }


        public class PaymentGroup
        {
            public class PaymentMethod
            {
                public int PaymentID { get; set; }
                public string PaymentName { get; set; }
                public string PaymentValue { get; set; }
            }
            public string GroupMethodValue { get; set; }
            public string GroupMethodName { get; set; }

            public string Icon { get; set; }
            private string Path = "/Assets/Icons/Payment";

            public string IconPath { get { return string.Format("{0}/{1}.svg", Path, Icon); } }
            public string IconPathSelected { get { return string.Format("{0}/{1}-2.svg", Path, Icon); } }

            public List<PaymentMethod> Payments { get; set; }
        }

        private OrderViewModel _orderVM;
        public PaymentView(OrderViewModel orderVM)
        {
            InitializeComponent();
            DataContext = orderVM;
            _orderVM = orderVM;
            Unloaded += PaymentView_Unloaded;
            InitPaymentMethod();
        }

        private void InitPaymentMethod()
        {
            var payments = new PaymentService().GetAll();
            var paymentGroups = new List<PaymentGroup>();
            foreach (var payment in payments)
            {
                var paymentOfGroup = new PaymentMethod
                {
                    PaymentID = payment.PaymentID,
                    PaymentName = payment.PaymentName,
                    PaymentValue = payment.PaymentValue
                };

                if (payment.GroupMethodValue == "Cash")
                {
                    paymentGroups.Add(new PaymentGroup
                    {
                        GroupMethodName = payment.GroupMethodName,
                        GroupMethodValue = payment.PaymentValue,
                        Payments = new List<PaymentMethod> { paymentOfGroup },
                        Icon = "ico-cash"
                    });

                    continue;
                }

                if (payment.GroupMethodValue == "Transfer")
                {
                    var groupName = "QR";

                    var existGroup = paymentGroups.Where(item => item.GroupMethodValue == groupName).FirstOrDefault();
                    if (existGroup == null)
                    {
                        paymentGroups.Add(new PaymentGroup
                        {
                            GroupMethodName = groupName,
                            GroupMethodValue = groupName,
                            Payments = new List<PaymentMethod> { paymentOfGroup },
                            Icon = "ico-qrcode"
                        });

                        continue;
                    }

                    existGroup.Payments.Add(paymentOfGroup);
                }
            }

            paymentGroups.Add(new PaymentGroup
            {
                GroupMethodName = "VISA",
                GroupMethodValue = "VISA",
                Payments = new List<PaymentMethod>(),
                Icon = "ico-visa"
            });

            paymentGroups.Add(new PaymentGroup
            {
                GroupMethodName = "Ví điện tử",
                GroupMethodValue = "Ví điện tử",
                Payments = new List<PaymentMethod>(),
                Icon = "ico-wallet"
            });

            paymentGroups.Add(new PaymentGroup
            {
                GroupMethodName = "Voucher",
                GroupMethodValue = "Voucher",
                Payments = new List<PaymentMethod>(),
                Icon = "ico-voucher"
            });

            paymentGroups.Add(new PaymentGroup
            {
                GroupMethodName = "Tách đơn",
                GroupMethodValue = "Tách đơn",
                Payments = new List<PaymentMethod>(),
                Icon = "ico-desk"
            });

            PaymentList.ItemsSource = paymentGroups;
        }

        private void PaymentView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is OrderViewModel vm)
            {
                vm.Summary.CustomerPaid = 0;
            }
        }

        private async Task SyncInvoiceAsync()
        {
            try
            {
                var currentUser = SessionService.Instance.CurrentUser;
                var summary = _orderVM.Summary;
                var now = DateTime.UtcNow;
                var apiClient = new ApiClient();

                var productSaleRequest = _orderVM.CartItems.Select(item => new NewInvoiceProductSaleRequest
                {
                    YS_ProductID = item.ID,
                    QuantitySold = item.Quantity,
                    PriceSold = item.OriginalPrice,
                    TotalAmountReality = item.TotalPrice
                });

                if (productSaleRequest.Count() <= 0)
                {
                    ToastService.Error("Không có sản phâm để thanh toán");
                    return;
                }

                var payments = new List<NewInvoicePaymentRequest>();
                payments.Add(new NewInvoicePaymentRequest
                {
                    PaymentAmount = summary.TotalAmount,
                    PaymentOption = 43034
                });

                var requestData = new CreateNewInvoiceRequest
                {
                    YS_AccountID = currentUser.YS_AccountID,
                    YS_BranchID = currentUser.BranchID,
                    Cashier = currentUser.ID,
                    TotalAmount = summary.SubTotalAmount,
                    InvoiceAmount = summary.TotalAmount,
                    CustomerReceived = summary.CustomerPaid,
                    Notes = string.Empty,
                    VAT = "",
                    VATTaxAmount = summary.VATAmount,
                    PromotionAmount = summary.PromotionAmount,
                    ShippingFee = summary.ShippingFee,
                    CreatedDate = now,
                    ProductSales = productSaleRequest,
                    Payments = payments
                };

                if (_orderVM.Customer != null && _orderVM.Customer.ID != null)
                {
                    requestData.CustomerID = _orderVM.Customer.ID;
                }

                var response = await apiClient.PostAsync<CreateNewInvoiceResponse>("Invoice", requestData);
                if (response.IsSuccess == false)
                {
                    MessageBox.Show(response.Message);
                    return;
                }

                _orderVM.Clear();
                ToastService.Success(string.Format("Đơn hàng: #{0} đã được tạo", response.InvoiceNumber));
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            _ = SyncInvoiceAsync();
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

        private void Money_Click(object sender, RoutedEventArgs e)
        {
            var textBox = Keyboard.FocusedElement as TextBox;

            if (textBox == null)
                return;

            var text = (sender as Button)?.Content?.ToString();

            if (string.IsNullOrEmpty(text))
                return;

            int amount = text switch
            {
                "10k" => 10000,
                "20k" => 20000,
                "50k" => 50000,
                "100k" => 100000,
                "200k" => 200000,
                "500k" => 500000,
                "1000k" => 1000000,
                "2000k" => 2000000,
                _ => 0
            };

            textBox.Text = amount.ToString("N0");

            textBox.CaretIndex = textBox.Text.Length;

            textBox.Focus();
        }

        private void PaymentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var payment = (PaymentGroup)listBox.SelectedItem;

            switch(payment.GroupMethodValue.ToUpper())
            {
                case "CASH":
                    var cashView = new CashView();
                    cashView.DataContext = _orderVM.Summary;
                    PaymentContent.Content = cashView;
                    break;

                case "QR":
                    var qrView = new QRView(payment.Payments);
                    qrView.DataContext = _orderVM.Summary;
                    PaymentContent.Content = qrView;
                    break;

                default:
                    PaymentContent.Content = null;
                    break;
            }
        }
    }
}
