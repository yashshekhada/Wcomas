using Microsoft.EntityFrameworkCore;
using Wcomas.Data;
using Wcomas.Models;

namespace Wcomas.Services
{
    public class BrandService
    {
        private readonly IDbContextFactory<WcomasDbContext> _dbFactory;

        public BrandService(IDbContextFactory<WcomasDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.Brands.OrderBy(b => b.Name).ToListAsync();
        }

        public async Task CreateBrandAsync(Brand brand)
        {
            using var context = _dbFactory.CreateDbContext();
            context.Brands.Add(brand);
            await context.SaveChangesAsync();
        }

        public async Task UpdateBrandAsync(Brand brand)
        {
            using var context = _dbFactory.CreateDbContext();
            context.Brands.Update(brand);
            await context.SaveChangesAsync();
        }

        public async Task DeleteBrandAsync(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var brand = await context.Brands.FindAsync(id);
            if (brand != null)
            {
                context.Brands.Remove(brand);
                await context.SaveChangesAsync();
            }
        }
    }
}
