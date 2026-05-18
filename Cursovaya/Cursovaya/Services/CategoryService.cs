using Cursovaya.Models;
using Cursovaya.Repositories;

namespace Cursovaya.Services;

public class CategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Category>> GetActiveAsync()
    {
        return await _unitOfWork.Categories.GetActiveAsync();
    }

    public async Task<List<Category>> GetAllAsync()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return categories.OrderBy(x => x.Name).ToList();
    }

    public async Task<ServiceResult> AddAsync(string name, string description, User? admin)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterCategoryName"));
        }

        if (await _unitOfWork.Categories.GetByNameAsync(name) != null)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorCategoryAlreadyExists"));
        }

        await _unitOfWork.Categories.AddAsync(new Category
        {
            Name = name.Trim(),
            Description = description.Trim(),
            IsActive = true
        });

        await _unitOfWork.SaveChangesAsync();
        await WriteLogAsync(admin, "AddCategory", LocalizedStrings.Format("LogCategoryAdded", name.Trim()));
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(Category category, User? admin)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterCategoryName"));
        }

        var existing = await _unitOfWork.Categories.GetByIdAsync(category.Id);
        if (existing == null)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorCategoryNotFound"));
        }

        var duplicate = await _unitOfWork.Categories.GetByNameAsync(category.Name);
        if (duplicate != null && duplicate.Id != category.Id)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorCategoryAlreadyExists"));
        }

        existing.Name = category.Name.Trim();
        existing.Description = category.Description.Trim();
        existing.IsActive = category.IsActive;
        _unitOfWork.Categories.Update(existing);
        await _unitOfWork.SaveChangesAsync();
        await WriteLogAsync(admin, "UpdateCategory", LocalizedStrings.Format("LogCategoryUpdated", existing.Name));
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Category category, User? admin)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        var existing = await _unitOfWork.Categories.GetByIdAsync(category.Id);
        if (existing == null)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorCategoryNotFound"));
        }

        if (await _unitOfWork.Categories.IsUsedAsync(category.Id))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorCategoryInUse"));
        }

        _unitOfWork.Categories.Delete(existing);
        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();
        await WriteLogAsync(admin, "DeleteCategory", LocalizedStrings.Format("LogCategoryDeleted", existing.Name));
        return ServiceResult.Success();
    }

    private async Task WriteLogAsync(User? admin, string action, string description)
    {
        if (admin?.Role != UserRole.Admin)
        {
            return;
        }

        await _unitOfWork.AppLogs.AddAsync(new AppLog
        {
            Action = action,
            Description = description,
            AdminUserId = admin.Id,
            CreatedAt = DateTime.Now
        });
        await _unitOfWork.SaveChangesAsync();
    }
}
