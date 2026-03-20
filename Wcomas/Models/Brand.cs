using System.ComponentModel.DataAnnotations;

namespace Wcomas.Models
{
    public class Brand
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string? LogoUrl { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public List<Product> Products { get; set; } = new();
    }
}
