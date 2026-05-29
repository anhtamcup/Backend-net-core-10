namespace POS_App.Models.Response.Product
{
    public class SyncProductResponse: ResponseBase
    {
        public IEnumerable<YS_Product> Data { get; set; }
    }
}
