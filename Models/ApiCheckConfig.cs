using System.ComponentModel.DataAnnotations;

public class ApiCheckConfig
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Url { get; set; }
}
