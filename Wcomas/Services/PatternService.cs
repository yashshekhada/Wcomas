using Microsoft.EntityFrameworkCore;
using Wcomas.Data;
using Wcomas.Models;

namespace Wcomas.Services;

public class PatternService
{
    private readonly IDbContextFactory<WcomasDbContext> _dbFactory;

    public PatternService(IDbContextFactory<WcomasDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Pattern>> GetAllPatternsAsync()
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        return await context.Patterns.OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task CreatePatternAsync(Pattern p)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.Patterns.Add(p);
        await context.SaveChangesAsync();
    }

    public async Task UpdatePatternAsync(Pattern p)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        context.Patterns.Update(p);
        await context.SaveChangesAsync();
    }

    public async Task DeletePatternAsync(int id)
    {
        using var context = await _dbFactory.CreateDbContextAsync();
        var p = await context.Patterns.FindAsync(id);
        if (p != null) {
            context.Patterns.Remove(p);
            await context.SaveChangesAsync();
        }
    }
}
