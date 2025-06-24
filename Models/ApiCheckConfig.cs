using System.ComponentModel.DataAnnotations;

public class ApiCheckConfig
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Url { get; set; } = string.Empty;
    [Required]
    public string Type { get; set; } = string.Empty; // "Website", "API", or "Server"
}
