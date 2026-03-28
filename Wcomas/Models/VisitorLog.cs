using System.ComponentModel.DataAnnotations;

namespace Wcomas.Models;

public class VisitorLog
{
    public int Id { get; set; }
    
    [Required]
    public string IpAddress { get; set; } = string.Empty;
    
    public string UserAgent { get; set; } = string.Empty;
    
    public string CurrentUrl { get; set; } = string.Empty;
    
    public string Country { get; set; } = "Unknown";
    public string City { get; set; } = "Unknown";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsLocated { get; set; }
    
    public bool IsMobile => UserAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase);
}
