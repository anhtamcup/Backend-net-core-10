using POS_App.Dto;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace POS_App.ViewModels
{
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

            AddCart();
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