using System;

public partial class ApiStatus
{
    public string ApiName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
    public string? ErrorMessage { get; set; }
    public string ServerIp { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Ensure this property exists
}
