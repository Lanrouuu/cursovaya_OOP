using Cursovaya.Models;
using Cursovaya.Repositories;

namespace Cursovaya.Services;

public class UserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _unitOfWork.Users.GetAllOrderedAsync();
    }

    public async Task<ServiceResult> UpdateProfileAsync(User currentUser, string userName, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return ServiceResult.Fail("Введите имя.");
        }

        if (!AuthService.IsEmailValid(email))
        {
            return ServiceResult.Fail("Введите корректный email.");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return ServiceResult.Fail("Введите телефон.");
        }

        if (await _unitOfWork.Users.IsEmailTakenAsync(email, currentUser.Id))
        {
            return ServiceResult.Fail("Email уже занят другим пользователем.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(currentUser.Id);
        if (user == null)
        {
            return ServiceResult.Fail("Пользователь не найден.");
        }

        user.UserName = userName.Trim();
        user.Email = email.Trim();
        user.PhoneNumber = phone.Trim();
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        currentUser.UserName = user.UserName;
        currentUser.Email = user.Email;
        currentUser.PhoneNumber = user.PhoneNumber;
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetBlockedAsync(User user, bool isBlocked)
    {
        var existing = await _unitOfWork.Users.GetByIdAsync(user.Id);
        if (existing == null)
        {
            return ServiceResult.Fail("Пользователь не найден.");
        }

        if (existing.Role == UserRole.Admin)
        {
            return ServiceResult.Fail("Администратора нельзя заблокировать.");
        }

        existing.IsBlocked = isBlocked;
        _unitOfWork.Users.Update(existing);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Success();
    }
}
