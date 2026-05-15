using Cursovaya.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cursovaya.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IAdvertisementRepository Advertisements { get; }
    ICategoryRepository Categories { get; }
    IRepository<FavoriteAdvertisement> Favorites { get; }
    IRepository<AppLog> AppLogs { get; }

    Task<int> SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    void Detach(object entity);
}
