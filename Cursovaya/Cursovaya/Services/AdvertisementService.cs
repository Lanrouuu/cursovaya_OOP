using Cursovaya.Models;
using Cursovaya.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Services;

public class AdvertisementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UndoRedoService _undoRedoService;

    public AdvertisementService(IUnitOfWork unitOfWork, UndoRedoService undoRedoService)
    {
        _unitOfWork = unitOfWork;
        _undoRedoService = undoRedoService;
    }

    public async Task<List<Advertisement>> GetFilteredAsync(AdvertisementFilter filter, bool includeInactive)
    {
        var query = _unitOfWork.Advertisements.QueryWithDetails();

        if (!includeInactive)
        {
            query = query.Where(x => x.Status == AdvertisementStatus.Active);
        }
        else if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }
        else
        {
            query = query.Where(x => x.Status != AdvertisementStatus.Deleted);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.Trim().ToLower();
            query = query.Where(x =>
                x.Title.ToLower().Contains(search) ||
                x.ShortDescription.ToLower().Contains(search) ||
                x.FullDescription.ToLower().Contains(search) ||
                x.City.ToLower().Contains(search) ||
                (x.User != null && x.User.UserName.ToLower().Contains(search)));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= filter.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            var city = filter.City.Trim().ToLower();
            query = query.Where(x => x.City.ToLower().Contains(city));
        }

        if (filter.Condition.HasValue)
        {
            query = query.Where(x => x.Condition == filter.Condition.Value);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= filter.DateFrom.Value);
        }

        query = filter.SortMode switch
        {
            "price_asc" => query.OrderBy(x => x.Price),
            "price_desc" => query.OrderByDescending(x => x.Price),
            "title" => query.OrderBy(x => x.Title),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Advertisement?> GetByIdAsync(int id)
    {
        return await _unitOfWork.Advertisements.GetByIdWithDetailsAsync(id);
    }

    public async Task<List<Advertisement>> GetByUserAsync(int userId)
    {
        return await _unitOfWork.Advertisements.GetByUserAsync(userId);
    }

    public async Task<ServiceResult<Advertisement>> AddAsync(Advertisement advertisement, User? currentUser, bool registerUndo = true)
    {
        if (currentUser == null)
        {
            return ServiceResult<Advertisement>.Fail("Добавлять объявления может только зарегистрированный пользователь.");
        }

        var validation = ValidateAdvertisement(advertisement);
        if (!validation.IsSuccess)
        {
            return ServiceResult<Advertisement>.Fail(validation.ErrorMessage);
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        advertisement.UserId = currentUser.Id;
        advertisement.Status = AdvertisementStatus.Active;
        advertisement.CreatedAt = DateTime.Now;
        advertisement.UpdatedAt = null;

        await _unitOfWork.Advertisements.AddAsync(advertisement);
        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();

        if (registerUndo)
        {
            _undoRedoService.AddAction(new AddedAdvertisementAction(this, advertisement.Id));
        }

        return ServiceResult<Advertisement>.Success(advertisement);
    }

    public async Task<ServiceResult> UpdateAsync(Advertisement changedAdvertisement, User? currentUser, bool registerUndo = true)
    {
        var existing = await _unitOfWork.Advertisements.GetByIdWithDetailsAsync(changedAdvertisement.Id);
        if (existing == null)
        {
            return ServiceResult.Fail("Объявление не найдено.");
        }

        if (!CanEdit(existing, currentUser))
        {
            return ServiceResult.Fail("Можно редактировать только свои объявления.");
        }

        var validation = ValidateAdvertisement(changedAdvertisement);
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var oldSnapshot = CloneAdvertisement(existing);
        var newSnapshot = CloneAdvertisement(changedAdvertisement);
        newSnapshot.Status = existing.Status;
        newSnapshot.UserId = existing.UserId;
        newSnapshot.CreatedAt = existing.CreatedAt;

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        CopyEditableFields(existing, changedAdvertisement);
        existing.UpdatedAt = DateTime.Now;
        _unitOfWork.Advertisements.Update(existing);
        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();

        if (registerUndo)
        {
            _undoRedoService.AddAction(new EditedAdvertisementAction(this, oldSnapshot, newSnapshot));
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int advertisementId, User? currentUser, bool registerUndo = true)
    {
        var advertisement = await _unitOfWork.Advertisements.GetByIdWithDetailsAsync(advertisementId);
        if (advertisement == null)
        {
            return ServiceResult.Fail("Объявление не найдено.");
        }

        if (!CanEdit(advertisement, currentUser))
        {
            return ServiceResult.Fail("Можно удалить только своё объявление.");
        }

        var oldSnapshot = CloneAdvertisement(advertisement);
        await SetStatusInternalAsync(advertisementId, AdvertisementStatus.Deleted);

        if (registerUndo)
        {
            _undoRedoService.AddAction(new DeletedAdvertisementAction(this, oldSnapshot));
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ChangeStatusAsync(int advertisementId, AdvertisementStatus status, User? currentUser)
    {
        if (currentUser?.Role != UserRole.Admin)
        {
            return ServiceResult.Fail("Менять статус объявления может только администратор.");
        }

        await SetStatusInternalAsync(advertisementId, status);
        await _unitOfWork.AppLogs.AddAsync(new AppLog
        {
            Action = "ChangeStatus",
            Description = $"Статус объявления #{advertisementId} изменён на {status}.",
            AdminUserId = currentUser.Id,
            CreatedAt = DateTime.Now
        });
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Success();
    }

    public async Task SetStatusInternalAsync(int advertisementId, AdvertisementStatus status)
    {
        var advertisement = await _unitOfWork.Advertisements.GetByIdAsync(advertisementId);
        if (advertisement == null)
        {
            return;
        }

        advertisement.Status = status;
        advertisement.UpdatedAt = DateTime.Now;
        _unitOfWork.Advertisements.Update(advertisement);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreSnapshotInternalAsync(Advertisement snapshot)
    {
        var advertisement = await _unitOfWork.Advertisements.GetByIdAsync(snapshot.Id);
        if (advertisement == null)
        {
            return;
        }

        CopyAllFields(advertisement, snapshot);
        _unitOfWork.Advertisements.Update(advertisement);
        await _unitOfWork.SaveChangesAsync();
    }

    private static bool CanEdit(Advertisement advertisement, User? currentUser)
    {
        if (currentUser == null)
        {
            return false;
        }

        return currentUser.Role == UserRole.Admin || advertisement.UserId == currentUser.Id;
    }

    private static ServiceResult ValidateAdvertisement(Advertisement advertisement)
    {
        if (string.IsNullOrWhiteSpace(advertisement.Title))
        {
            return ServiceResult.Fail("Введите название объявления.");
        }

        if (string.IsNullOrWhiteSpace(advertisement.ShortDescription) ||
            string.IsNullOrWhiteSpace(advertisement.FullDescription))
        {
            return ServiceResult.Fail("Введите описание объявления.");
        }

        if (advertisement.Price < 0)
        {
            return ServiceResult.Fail("Цена не может быть отрицательной.");
        }

        if (advertisement.CategoryId <= 0)
        {
            return ServiceResult.Fail("Выберите категорию.");
        }

        if (string.IsNullOrWhiteSpace(advertisement.City))
        {
            return ServiceResult.Fail("Введите город.");
        }

        if (!AuthService.IsEmailValid(advertisement.SellerContactEmail))
        {
            return ServiceResult.Fail("Введите корректный контактный email.");
        }

        if (string.IsNullOrWhiteSpace(advertisement.SellerContactPhone))
        {
            return ServiceResult.Fail("Введите контактный телефон.");
        }

        return ServiceResult.Success();
    }

    private static Advertisement CloneAdvertisement(Advertisement advertisement)
    {
        return new Advertisement
        {
            Id = advertisement.Id,
            Title = advertisement.Title,
            ShortDescription = advertisement.ShortDescription,
            FullDescription = advertisement.FullDescription,
            Price = advertisement.Price,
            City = advertisement.City,
            Condition = advertisement.Condition,
            Status = advertisement.Status,
            CreatedAt = advertisement.CreatedAt,
            UpdatedAt = advertisement.UpdatedAt,
            ImagePath = advertisement.ImagePath,
            SellerContactEmail = advertisement.SellerContactEmail,
            SellerContactPhone = advertisement.SellerContactPhone,
            UserId = advertisement.UserId,
            CategoryId = advertisement.CategoryId
        };
    }

    private static void CopyEditableFields(Advertisement target, Advertisement source)
    {
        target.Title = source.Title.Trim();
        target.ShortDescription = source.ShortDescription.Trim();
        target.FullDescription = source.FullDescription.Trim();
        target.Price = source.Price;
        target.City = source.City.Trim();
        target.Condition = source.Condition;
        target.CategoryId = source.CategoryId;
        target.ImagePath = source.ImagePath;
        target.SellerContactEmail = source.SellerContactEmail.Trim();
        target.SellerContactPhone = source.SellerContactPhone.Trim();
    }

    private static void CopyAllFields(Advertisement target, Advertisement source)
    {
        CopyEditableFields(target, source);
        target.Status = source.Status;
        target.UserId = source.UserId;
        target.CreatedAt = source.CreatedAt;
        target.UpdatedAt = source.UpdatedAt;
    }
}
