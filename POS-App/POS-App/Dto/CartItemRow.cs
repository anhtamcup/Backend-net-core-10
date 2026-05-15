using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_App.Dto
{
    public class CartItemRow : INotifyPropertyChanged
    {
        private int _id;
        private int _index;
        private string _code = string.Empty;
        private string _name = string.Empty;
        private decimal _originalPrice;
        private decimal _vatPrice;
        private decimal _discountPrice;
        private int _quantity;
        private string _note = string.Empty;
        private string _unit = string.Empty;
        private bool _isSelected;

        public int ID
        {
            get => _id;
            set => SetField(ref _id, value);
        }

        public int Index
        {
            get => _index;
            set => SetField(ref _index, value);
        }

        public string Code
        {
            get => _code;
            set => SetField(ref _code, value);
        }

        public string Name
        {
            get => _name;
            set => SetField(ref _name, value);
        }

        public decimal OriginalPrice
        {
            get => _originalPrice;
            set
            {
                if (SetField(ref _originalPrice, value))
                    OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal VATPrice
        {
            get => _vatPrice;
            set
            {
                if (SetField(ref _vatPrice, value))
                    OnPropertyChanged(nameof(VATPrice));
            }
        }

        public decimal DiscountPrice
        {
            get => _discountPrice;
            set
            {
                if (SetField(ref _discountPrice, value))
                    OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetField(ref _quantity, value))
                    OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal TotalPrice =>
            (OriginalPrice * Quantity) - DiscountPrice;

        public string Note
        {
            get => _note;
            set => SetField(ref _note, value);
        }

        public string Unit
        {
            get => _unit;
            set => SetField(ref _unit, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(
            ref T field,
            T value,
            [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}