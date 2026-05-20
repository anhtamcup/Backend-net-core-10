namespace POS_App.Models
{
    public class UserInfo
    {
        public int ID { get; set; }
        public int YS_AccountID { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
}
