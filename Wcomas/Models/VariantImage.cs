namespace Wcomas.Models;

public class VariantImage
{
    public int Id { get; set; }
    public int VariantId { get; set; }
    public ProductVariant? Variant { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
