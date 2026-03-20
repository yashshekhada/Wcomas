using Microsoft.EntityFrameworkCore;
using Wcomas.Models;

namespace Wcomas.Data;

public class WcomasDbContext : DbContext
{
    public WcomasDbContext(DbContextOptions<WcomasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Brand> Brands { get; set; } = null!;
    public DbSet<Inquiry> Inquiries { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
    public DbSet<Pattern> Patterns { get; set; } = null!;
    public DbSet<VariantImage> VariantImages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ProductVariant → Pattern (optional FK)
        modelBuilder.Entity<ProductVariant>()
            .HasOne(v => v.Pattern)
            .WithMany()
            .HasForeignKey(v => v.PatternId)
            .OnDelete(DeleteBehavior.SetNull);

        // ProductVariant → VariantImages
        modelBuilder.Entity<VariantImage>()
            .HasOne(vi => vi.Variant)
            .WithMany(v => v.Images)
            .HasForeignKey(vi => vi.VariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
