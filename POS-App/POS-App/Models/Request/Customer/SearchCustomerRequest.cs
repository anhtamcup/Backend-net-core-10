namespace POS_App.Models.Request.Customer
{
    public class SearchCustomerRequest
    {
        public string KeySearch { get; set; } = string.Empty;

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
