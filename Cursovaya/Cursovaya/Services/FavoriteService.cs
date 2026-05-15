using Cursovaya.Models;
using Cursovaya.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Services;

public class FavoriteService
{
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<HashSet<int>> GetFavoriteIdsAsync(int userId)
    {
        var ids = await _unitOfWork.Favorites
            .Query()
            .Where(x => x.UserId == userId)
            .Select(x => x.AdvertisementId)
            .ToListAsync();
        return ids.ToHashSet();
    }

    public async Task<List<Advertisement>> GetFavoritesAsync(int userId)
    {
        return await _unitOfWork.Favorites
            .Query()
            .Where(x => x.UserId == userId)
            .Include(x => x.Advertisement!.User)
            .Include(x => x.Advertisement!.Category)
            .Include(x => x.Advertisement!.FavoriteAdvertisements)
            .Select(x => x.Advertisement!)
            .Where(a => a != null && a.Status == AdvertisementStatus.Active)
            .OrderByDescending(a => a.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    // Returns true if added, false if removed
    public async Task<bool> ToggleAsync(int userId, int advertisementId)
    {
        var existing = await _unitOfWork.Favorites
            .Query()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.AdvertisementId == advertisementId);

        if (existing != null)
        {
            _unitOfWork.Favorites.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
            return false;
        }

        await _unitOfWork.Favorites.AddAsync(new FavoriteAdvertisement
        {
            UserId = userId,
            AdvertisementId = advertisementId,
            CreatedAt = DateTime.Now
        });
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
