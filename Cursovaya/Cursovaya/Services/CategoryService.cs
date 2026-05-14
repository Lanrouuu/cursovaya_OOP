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

    public async Task<ServiceResult> AddAsync(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult.Fail("Введите название категории.");
        }

        if (await _unitOfWork.Categories.GetByNameAsync(name) != null)
        {
            return ServiceResult.Fail("Такая категория уже существует.");
        }

        await _unitOfWork.Categories.AddAsync(new Category
        {
            Name = name.Trim(),
            Description = description.Trim(),
            IsActive = true
        });

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return ServiceResult.Fail("Введите название категории.");
        }

        var existing = await _unitOfWork.Categories.GetByIdAsync(category.Id);
        if (existing == null)
        {
            return ServiceResult.Fail("Категория не найдена.");
        }

        var duplicate = await _unitOfWork.Categories.GetByNameAsync(category.Name);
        if (duplicate != null && duplicate.Id != category.Id)
        {
            return ServiceResult.Fail("Такая категория уже существует.");
        }

        existing.Name = category.Name.Trim();
        existing.Description = category.Description.Trim();
        existing.IsActive = category.IsActive;
        _unitOfWork.Categories.Update(existing);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Category category)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        var existing = await _unitOfWork.Categories.GetByIdAsync(category.Id);
        if (existing == null)
        {
            return ServiceResult.Fail("Категория не найдена.");
        }

        if (await _unitOfWork.Categories.IsUsedAsync(category.Id))
        {
            return ServiceResult.Fail("Нельзя удалить категорию, которая используется в объявлениях.");
        }

        _unitOfWork.Categories.Delete(existing);
        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();
        return ServiceResult.Success();
    }
}
