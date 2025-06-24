namespace MyAspNetCoreApp.Models
{
    public class ApiStatus
    {
        public string ApiName { get; set; }
        public string Status { get; set; }
        public DateTime LastChecked { get; set; }
        public string ErrorMessage { get; set; }
    }
}