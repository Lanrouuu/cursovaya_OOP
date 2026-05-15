using Cursovaya.Models;
using Cursovaya.Repositories;

namespace Cursovaya.Services;

public class UserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmailService _emailService;

    public UserService(IUnitOfWork unitOfWork, EmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _unitOfWork.Users.GetAllOrderedAsync();
    }

    public async Task<ServiceResult> UpdateProfileAsync(User currentUser, string userName, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterName"));
        }

        if (userName.Trim().Length < 2 || userName.Trim().Length > 50)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorNameLength"));
        }

        if (!AuthService.IsEmailValid(email))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterValidEmail"));
        }

        if (!AuthService.IsPhoneValid(phone))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterValidPhone"));
        }

        if (await _unitOfWork.Users.IsEmailTakenAsync(email, currentUser.Id))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEmailUsedByAnotherUser"));
        }

        var user = await _unitOfWork.Users.GetByIdAsync(currentUser.Id);
        if (user == null)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorUserNotFound"));
        }

        user.UserName = userName.Trim();
        user.Email = email.Trim();
        user.PhoneNumber = phone.Trim();
        user.UpdatedAt = DateTime.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        currentUser.UserName = user.UserName;
        currentUser.Email = user.Email;
        currentUser.PhoneNumber = user.PhoneNumber;
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ChangePasswordAsync(User currentUser, string currentPassword, string newPassword, string confirmNewPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterCurrentPassword"));
        }

        var user = await _unitOfWork.Users.GetByIdAsync(currentUser.Id);
        if (user == null)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorUserNotFound"));
        }

        if (!PasswordService.VerifyPassword(currentPassword, user.PasswordHash))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorInvalidCurrentPassword"));
        }

        if (newPassword.Length < 6)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorNewPasswordLength"));
        }

        if (newPassword != confirmNewPassword)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorNewPasswordsMismatch"));
        }

        user.PasswordHash = PasswordService.HashPassword(newPassword);
        user.UpdatedAt = DateTime.Now;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetBlockedAsync(User user, bool isBlocked)
    {
        var existing = await _unitOfWork.Users.GetByIdAsync(user.Id);
        if (existing == null)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorUserNotFound"));
        }

        if (existing.Role == UserRole.Admin)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorCannotBlockAdmin"));
        }

        existing.IsBlocked = isBlocked;
        _unitOfWork.Users.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        if (isBlocked)
            _ = _emailService.SendAccountBlockedAsync(existing.Email, existing.UserName);
        else
            _ = _emailService.SendAccountUnblockedAsync(existing.Email, existing.UserName);

        return ServiceResult.Success();
    }
}
