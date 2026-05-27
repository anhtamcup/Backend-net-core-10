namespace POS_App.Models.Response.Auth
{
    public class LoginResponse: ResponseBase
    {
        public class BranchAccess
        {
            public int YS_BranchID { get; set; }
            public string BranchName { get; set; }
        }

        public string Token { get; set; }

        public int YS_UserID { get; set; }
        public int YS_AccountID { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string StoreName { get; set; }
        public IEnumerable<BranchAccess> Branchs { get; set; }
    }
}
