using Cursovaya.Data;
using Cursovaya.Models;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Repositories;

public class AdvertisementRepository : Repository<Advertisement>, IAdvertisementRepository
{
    public AdvertisementRepository(AppDbContext context) : base(context)
    {
    }

    public IQueryable<Advertisement> QueryWithDetails()
    {
        return DbSet
            .Include(x => x.User)
            .Include(x => x.Category);
    }

    public async Task<Advertisement?> GetByIdWithDetailsAsync(int id)
    {
        return await QueryWithDetails().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Advertisement>> GetActiveWithDetailsAsync()
    {
        return await QueryWithDetails()
            .Where(x => x.Status == AdvertisementStatus.Active)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Advertisement>> GetByUserAsync(int userId)
    {
        return await QueryWithDetails()
            .Where(x => x.UserId == userId && x.Status != AdvertisementStatus.Deleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
