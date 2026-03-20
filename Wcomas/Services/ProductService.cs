using Microsoft.EntityFrameworkCore;
using Wcomas.Data;
using Wcomas.Models;

namespace Wcomas.Services
{
    public class ProductService
    {
        private readonly IDbContextFactory<WcomasDbContext> _dbFactory;

        public ProductService(IDbContextFactory<WcomasDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateProductAsync(Product product)
        {
            using var context = _dbFactory.CreateDbContext();
            context.Products.Add(product);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            using var context = _dbFactory.CreateDbContext();
            context.Products.Update(product);
            await context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var product = await context.Products.FindAsync(id);
            if (product != null)
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
            }
        }
    }
}
