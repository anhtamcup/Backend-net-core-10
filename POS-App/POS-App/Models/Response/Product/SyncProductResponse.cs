namespace POS_App.Models.Response.Product
{
    //public class ProductSync
    //{
    //    public int YS_ProductID { get; set; }
    //    public int YS_AccountID { get; set; }
    //    public string ProductCode { get; set; } = string.Empty;
    //    public string ProductName { get; set; } = string.Empty;
    //    public string ProductDescription { get; set; } = string.Empty;
    //    public string Barcodes { get; set; } = string.Empty;
    //    public decimal? RetailPrice { get; set; }
    //    public DateTime? CreatedDate { get; set; }
    //    public int? CreatedBy { get; set; }
    //    public DateTime? ModifiedDate { get; set; }
    //    public int? ModifiedBy { get; set; }
    //    public string ProductVAT { get; set; }
    //}

    public class SyncProductResponse: ResponseBase
    {
        public IEnumerable<YS_Product> Data { get; set; }
    }
}
