using Cursovaya.Data;
using Cursovaya.Models;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return await DbSet
            .Include(x => x.Advertisements)
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail);
    }

    public async Task<bool> IsEmailTakenAsync(string email, int? exceptUserId = null)
    {
        var normalizedEmail = email.Trim().ToLower();
        var query = DbSet.Where(x => x.Email.ToLower() == normalizedEmail);

        if (exceptUserId.HasValue)
        {
            query = query.Where(x => x.Id != exceptUserId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<User>> GetAllOrderedAsync()
    {
        return await DbSet
            .OrderBy(x => x.Role)
            .ThenBy(x => x.UserName)
            .ToListAsync();
    }
}
