using POS_App.Dto;
using System.Collections.ObjectModel;

namespace POS_App.ViewModels
{
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
}
