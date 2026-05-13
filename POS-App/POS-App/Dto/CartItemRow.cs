using System;
using System.Collections.Generic;
using System.Text;

namespace POS_App.Dto
{
    public class CartItemRow
    {
        public int ID { get; set; }
        public int Index { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string Note { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
