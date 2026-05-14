using Cursovaya.Models;

namespace Cursovaya.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<List<Category>> GetActiveAsync();
    Task<Category?> GetByNameAsync(string name);
    Task<bool> IsUsedAsync(int categoryId);
}
