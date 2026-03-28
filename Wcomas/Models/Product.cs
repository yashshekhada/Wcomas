using System.ComponentModel.DataAnnotations;

namespace Wcomas.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Code { get; set; } // SKU/Code
        
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; } // Main thumbnail/image
        
        public decimal Price { get; set; }
        
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        
        public int? BrandId { get; set; }
        public Brand? Brand { get; set; }
        
        public int? PatternId { get; set; }
        public Pattern? Pattern { get; set; }

        public string? Colors { get; set; } // Comma-separated hex values

        public string? YoutubeLink { get; set; }
        
        public string? ModelUrl { get; set; } // 3D model .glb/.gltf

        public List<ProductImage> Images { get; set; } = new();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
