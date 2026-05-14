using System.Collections.ObjectModel;

namespace POS_App.Dto
{
    public class CartSummary
    {
        public int TotalItem { get; set; }
        public decimal SubTotalAmount { get; set; }
        public decimal PromotionAmount { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
