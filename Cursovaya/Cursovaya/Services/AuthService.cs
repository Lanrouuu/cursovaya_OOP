using Cursovaya.Models;
using Cursovaya.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Cursovaya.Services;

public class AuthService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public User? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;

    public event Action? CurrentUserChanged;

    public async Task<ServiceResult<User>> LoginAsync(string email, string password)
    {
        var normalizedEmail = email?.Trim() ?? string.Empty;
        var normalizedPassword = password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(normalizedPassword))
        {
            return ServiceResult<User>.Fail("Введите email и пароль.");
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail);
        if (user == null || !PasswordService.VerifyPassword(normalizedPassword, user.PasswordHash))
        {
            return ServiceResult<User>.Fail("Неверный email или пароль.");
        }

        if (user.IsBlocked)
        {
            return ServiceResult<User>.Fail("Пользователь заблокирован.");
        }

        CurrentUser = user;
        CurrentUserChanged?.Invoke();
        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult<User>> RegisterAsync(
        string userName,
        string email,
        string phone,
        string password,
        string confirmPassword)
    {
        var normalizedUserName = userName?.Trim() ?? string.Empty;
        var normalizedEmail = email?.Trim() ?? string.Empty;
        var normalizedPhone = phone?.Trim() ?? string.Empty;
        var normalizedPassword = password ?? string.Empty;
        var normalizedConfirmPassword = confirmPassword ?? string.Empty;

        var validation = await ValidateRegistrationAsync(
            normalizedUserName,
            normalizedEmail,
            normalizedPhone,
            normalizedPassword,
            normalizedConfirmPassword);
        if (!validation.IsSuccess)
        {
            return ServiceResult<User>.Fail(validation.ErrorMessage);
        }

        var user = new User
        {
            UserName = normalizedUserName,
            Email = normalizedEmail,
            PhoneNumber = normalizedPhone,
            PasswordHash = PasswordService.HashPassword(normalizedPassword),
            Role = UserRole.User,
            CreatedAt = DateTime.Now
        };

        try
        {
            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _unitOfWork.Detach(user);
            return ServiceResult<User>.Fail("Не удалось создать пользователя. Проверьте, что email не занят, и попробуйте еще раз.");
        }

        CurrentUser = user;
        CurrentUserChanged?.Invoke();
        return ServiceResult<User>.Success(user);
    }

    public void Logout()
    {
        CurrentUser = null;
        CurrentUserChanged?.Invoke();
    }

    private async Task<ServiceResult> ValidateRegistrationAsync(
        string userName,
        string email,
        string phone,
        string password,
        string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return ServiceResult.Fail("Введите имя пользователя.");
        }

        if (!IsEmailValid(email))
        {
            return ServiceResult.Fail("Введите корректный email.");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            return ServiceResult.Fail("Введите телефон.");
        }

        if (password.Length < 6)
        {
            return ServiceResult.Fail("Пароль должен содержать минимум 6 символов.");
        }

        if (password != confirmPassword)
        {
            return ServiceResult.Fail("Пароли не совпадают.");
        }

        if (await _unitOfWork.Users.IsEmailTakenAsync(email))
        {
            return ServiceResult.Fail("Пользователь с таким email уже есть.");
        }

        return ServiceResult.Success();
    }

    public static bool IsEmailValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
