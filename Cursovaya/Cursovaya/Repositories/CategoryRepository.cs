using Cursovaya.Data;
using Cursovaya.Models;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Category>> GetActiveAsync()
    {
        return await DbSet
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        var normalizedName = name.Trim().ToLower();
        return await DbSet.FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedName);
    }

    public async Task<bool> IsUsedAsync(int categoryId)
    {
        return await Context.Advertisements.AnyAsync(x => x.CategoryId == categoryId);
    }
}
