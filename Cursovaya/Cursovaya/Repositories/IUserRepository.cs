using Cursovaya.Models;

namespace Cursovaya.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsEmailTakenAsync(string email, int? exceptUserId = null);
    Task<List<User>> GetAllOrderedAsync();
}
