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
    public DbSet<Pattern> Patterns { get; set; } = null!;
    public DbSet<ProductImage> ProductImages { get; set; } = null!;
    public DbSet<VisitorLog> VisitorLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product → Pattern (optional FK)
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Pattern)
            .WithMany()
            .HasForeignKey(p => p.PatternId)
            .OnDelete(DeleteBehavior.SetNull);

        // Product → ProductImages
        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
