using System.ComponentModel.DataAnnotations;

namespace Wcomas.Models;

public class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty; // Variant display name

    public string? SKU { get; set; } // Product code

    public decimal PriceAdjustment { get; set; } // Extra price above base

    // Pattern (nullable FK)
    public int? PatternId { get; set; }
    public Pattern? Pattern { get; set; }

    // Comma-separated hex color values, e.g. "#FFD700,#FFFF00"
    public string? Colors { get; set; }

    public string? YouTubeLink { get; set; }

    public string? ModelUrl { get; set; } // 3D model URL

    // Navigation: multiple images
    public List<VariantImage> Images { get; set; } = new();
}
