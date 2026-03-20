using System.ComponentModel.DataAnnotations;

namespace Wcomas.Models;

public class Pattern
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
