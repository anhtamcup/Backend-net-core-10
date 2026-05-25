namespace POS_App.Config
{
    public class AppConfig
    {
        public ApiSettings ApiSettings { get; set; } = new();
    }

    public class ApiSettings
    {
        public string BaseUrl { get; set; } = "";
        public int Timeout { get; set; }
    }
}
