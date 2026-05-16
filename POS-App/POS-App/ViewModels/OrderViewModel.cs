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
            Name = "Nguyễn Thượng Đế";
            Point = 25000;
        }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
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
            }
        }

        public void Recalculate(ObservableCollection<CartItemRow> cartItems)
        {
            var itemCount = 0;
            var promotionAmount = 0m;
            var subTotalAmount = 0m;
            var vatAmount = 0m;
            var totalAmount = 0m;

            foreach (var item in cartItems)
            {
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
            TotalAmount = totalAmount;
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

        public void Recalculate()
        {
            Summary.Recalculate(CartItems);
        }


        public void AddCart()
        {
            CartItems.Add(new CartItemRow
            {
                ID = 1000,
                Index = CartItems.Count + 1,
                Code = "00000",
                Name = "Nước suối Lavie",
                Quantity = 1,
                Unit = "Chai",
                OriginalPrice = 10000
            });

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