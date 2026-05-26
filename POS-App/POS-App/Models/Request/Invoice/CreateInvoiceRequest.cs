namespace POS_App.Models.Request.Invoice
{
    public class NewInvoicePaymentRequest
    {
        public Nullable<int> PaymentType { get; set; }
        public Nullable<int> PaymentOption { get; set; }
        public Nullable<int> PaymentStatus { get; set; }
        public Nullable<decimal> PaymentAmount { get; set; }
    }

    public class NewInvoiceProductSaleRequest
    {
        public int YS_ProductID { get; set; }
        public int QuantitySold { get; set; }
        public decimal PriceSold { get; set; }
        public string Discount { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string VAT { get; set; } = string.Empty;
        public Nullable<System.Guid> ProductVariantID { get; set; }
        public Nullable<int> BatchID { get; set; }
        public Nullable<int> WareHouseID { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public Nullable<System.DateTime> BatchExpiryDate { get; set; }
        public decimal TotalAmountReality { get; set; }
    }

    public class CreateNewInvoiceRequest
    {
        public int YS_AccountID { get; set; }
        public int YS_BranchID { get; set; }
        public Nullable<DateTime> InvoiceDate { get; set; }
        public Nullable<int> Cashier { get; set; }
        public Nullable<System.DateTime> PickupDate { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal InvoiceAmount { get; set; }
        public Nullable<decimal> CustomerReceived { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string VAT { get; set; } = string.Empty;
        public Nullable<decimal> VATTaxAmount { get; set; }
        public Nullable<decimal> PromotionAmount { get; set; }
        public string PromotionDiscount { get; set; } = string.Empty;
        public Nullable<decimal> PromotionDiscountAmount { get; set; }
        public Nullable<decimal> IncludedVAT { get; set; }
        public Nullable<decimal> IncludedVATAmount { get; set; }
        public Nullable<int> ShippingAddressId { get; set; }
        public Nullable<decimal> ShippingFee { get; set; }
        public Nullable<int> ShiftID { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }

        public IEnumerable<NewInvoicePaymentRequest> Payments { get; set; }
        public IEnumerable<NewInvoiceProductSaleRequest> ProductSales { get; set; }
    }
}
