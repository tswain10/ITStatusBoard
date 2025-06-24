using System;

public class ApiStatusHistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }
}
