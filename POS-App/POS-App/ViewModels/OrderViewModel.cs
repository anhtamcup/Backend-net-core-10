using POS_App.Dto;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace POS_App.ViewModels
{
    public class CustomerInfoViewModel : BaseViewModel
    {
        public CustomerInfoViewModel()
        {
        }

        private int? _id;
        public int? ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _memberRank = "";
        public string MemberRank
        {
            get => _memberRank;
            set => SetProperty(ref _memberRank, value);
        }

        private string _phone = "";
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private int _point;
        public int Point
        {
            get => _point;
            set => SetProperty(ref _point, value);
        }
    }

    public class CartSummaryViewModel : BaseViewModel
    {
        private int _itemCount;
        public int ItemCount
        {
            get => _itemCount;
            set
            {
                _itemCount = value;
                OnPropertyChanged();
            }
        }

        private decimal _subTotalAmount;
        public decimal SubTotalAmount
        {
            get => _subTotalAmount;
            set
            {
                _subTotalAmount = value;
                OnPropertyChanged();
            }
        }

        private decimal _promotionAmount;
        public decimal PromotionAmount
        {
            get => _promotionAmount;
            set
            {
                _promotionAmount = value;
                OnPropertyChanged();
            }
        }

        private decimal _vatAmout;
        public decimal VATAmount
        {
            get => _vatAmout;
            set
            {
                _vatAmout = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChangeAmount));
            }
        }

        private decimal _customerPaid;
        public decimal CustomerPaid
        {
            get => _customerPaid;
            set
            {
                if (_customerPaid == value)
                    return;

                _customerPaid = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(ChangeAmount));
            }
        }

        public decimal ChangeAmount
        {
            get
            {
                return Math.Max(0, CustomerPaid - TotalAmount);
            }
        }

        public void SetCustomerPaid(decimal _amount)
        {
            CustomerPaid = _amount;

        }

        private decimal _shippingFee;
        public decimal ShippingFee
        {
            get => _shippingFee;
            set
            {
                _shippingFee = value;
                OnPropertyChanged();
            }
        }

        public void Recalculate(ObservableCollection<CartItemRow> cartItems)
        {
            var itemCount = 0;
            var promotionAmount = 0m;
            var subTotalAmount = 0m;
            var vatAmount = 0m;
            var totalAmount = 0m;

            for (int i = cartItems.Count - 1; i >= 0; i--)
            {
                if (cartItems[i].Quantity <= 0)
                {
                    cartItems.RemoveAt(i);
                }
            }

            var index = 1;
            foreach (var item in cartItems)
            {
                item.Index = index;
                index++;
                itemCount += item.Quantity;
                promotionAmount += item.DiscountPrice;
                subTotalAmount += item.Quantity * item.OriginalPrice;
                totalAmount += item.TotalPrice;
                vatAmount += item.VATPrice;
            }

            ItemCount = itemCount;
            SubTotalAmount = subTotalAmount;
            PromotionAmount = promotionAmount;
            VATAmount = vatAmount;
            ShippingFee = 0m;

            TotalAmount = totalAmount + vatAmount - promotionAmount + ShippingFee;
        }
    }

    public class OrderViewModel : BaseViewModel
    {
        public CustomerInfoViewModel Customer { get; }
            = new();

        public CartSummaryViewModel Summary { get; }
            = new();

        public ObservableCollection<CartItemRow> CartItems
        { get; } = new();

        public ICollectionView CartItemsView { get; }

        public OrderViewModel()
        {
            CartItemsView =
                CollectionViewSource
                .GetDefaultView(CartItems);

            CartItemsView.SortDescriptions.Add(
                new SortDescription(
                    nameof(CartItemRow.Index),
                    ListSortDirection.Descending));
        }

        public void Clear()
        {
            CartItems.Clear();
            SetCustomer(null);
            Summary.Recalculate(CartItems);
        }

        public void Recalculate()
        {
            Summary.Recalculate(CartItems);
        }

        public void SetCustomer(CustomerInfoViewModel customer = null)
        {
            if (customer == null)
            {
                Customer.Name = string.Empty;
                Customer.Point = 0;
                Customer.Phone = string.Empty;
                Customer.ID = null;
                Customer.MemberRank = string.Empty;
            }
            else
            {
                Customer.Name = customer.Name;
                Customer.Point = customer.Point;
                Customer.Phone = customer.Phone;
                Customer.ID = customer.ID;
                Customer.MemberRank = customer.MemberRank;
            }
        }

        public void AddCart(CartItemRow cart)
        {
            var existCart = CartItems.Where(item => item.ID == cart.ID).FirstOrDefault();
            if (existCart != null)
            {
                existCart.Quantity += cart.Quantity;
            }
            else
            {
                CartItems.Insert(0, cart);
            }

            Summary.Recalculate(CartItems);
        }

        public void Increase(CartItemRow item)
        {
            item.Quantity++;

            Summary.Recalculate(CartItems);
        }

        public void Decrease(CartItemRow item)
        {
            item.Quantity--;

            if (item.Quantity <= 0)
                CartItems.Remove(item);

            Summary.Recalculate(CartItems);
        }

        public void Remove(CartItemRow item)
        {
            CartItems.Remove(item);

            Summary.Recalculate(CartItems);
        }
    }
}