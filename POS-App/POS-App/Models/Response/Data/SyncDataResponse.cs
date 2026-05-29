namespace POS_App.Models.Response.Data
{
    public class SyncDataResponse: ResponseBase
    {
        public IEnumerable<YS_Product> Products { get; set; }
        public IEnumerable<YS_Payment> Payments { get; set; }
    }
}
