namespace POS_App.Models.Response.Customer
{
    public class CustomerInfor
    {
        public int YS_CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public int LoyaltyPoint { get; set; }
        public string MemberRank { get; set; }
    }

    public class SearchCustomerResponse: ResponseBase
    {
        public IEnumerable<CustomerInfor> Customers { get; set; }
    }
}
