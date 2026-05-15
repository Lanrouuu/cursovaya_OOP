using Cursovaya.Data;
using Cursovaya.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Cursovaya.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Advertisements = new AdvertisementRepository(context);
        Categories = new CategoryRepository(context);
        Favorites = new Repository<FavoriteAdvertisement>(context);
        AppLogs = new Repository<AppLog>(context);
    }

    public IUserRepository Users { get; }
    public IAdvertisementRepository Advertisements { get; }
    public ICategoryRepository Categories { get; }
    public IRepository<FavoriteAdvertisement> Favorites { get; }
    public IRepository<AppLog> AppLogs { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public void Detach(object entity)
    {
        _context.Entry(entity).State = EntityState.Detached;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
