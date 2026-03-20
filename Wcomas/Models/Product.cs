using System.ComponentModel.DataAnnotations;

namespace Wcomas.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public decimal BasePrice { get; set; }
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        
        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }
        
        public List<ProductVariant> Variants { get; set; } = new();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
