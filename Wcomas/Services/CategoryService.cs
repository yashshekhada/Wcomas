using Microsoft.EntityFrameworkCore;
using Wcomas.Data;
using Wcomas.Models;

namespace Wcomas.Services;

public class CategoryService
{
    private readonly IDbContextFactory<WcomasDbContext> _contextFactory;

    public CategoryService(IDbContextFactory<WcomasDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Categories.FindAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Entry(category).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var category = await context.Categories.FindAsync(id);
        if (category != null)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }
    }
}
