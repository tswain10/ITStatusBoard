using System;

public class ApiStatus
{
    public string ApiName { get; set; }
    public string Status { get; set; }
    public DateTime LastChecked { get; set; }
    public string ErrorMessage { get; set; }
    public string ServerIp { get; set; }
    public string Url { get; set; } // Add this property
}
