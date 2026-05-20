namespace POS_App.Models
{
    public class AuthSession
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string SessionId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
