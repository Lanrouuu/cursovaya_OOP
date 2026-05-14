using Cursovaya.Models;

namespace Cursovaya.Repositories;

public interface IAdvertisementRepository : IRepository<Advertisement>
{
    IQueryable<Advertisement> QueryWithDetails();
    Task<Advertisement?> GetByIdWithDetailsAsync(int id);
    Task<List<Advertisement>> GetActiveWithDetailsAsync();
    Task<List<Advertisement>> GetByUserAsync(int userId);
}
