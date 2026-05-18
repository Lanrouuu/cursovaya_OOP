using Cursovaya.Models;
using Cursovaya.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Cursovaya.Services;

public class AuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmailService _emailService;

    public AuthService(IUnitOfWork unitOfWork, EmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
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
            return ServiceResult<User>.Fail(LocalizedStrings.Get("ErrorEnterEmailPassword"));
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail);
        if (user == null || !PasswordService.VerifyPassword(normalizedPassword, user.PasswordHash))
        {
            return ServiceResult<User>.Fail(LocalizedStrings.Get("ErrorInvalidEmailPassword"));
        }

        if (user.IsBlocked)
        {
            return ServiceResult<User>.Fail(LocalizedStrings.Get("ErrorUserBlocked"));
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
            return ServiceResult<User>.Fail(LocalizedStrings.Get("ErrorCreateUserFailed"));
        }

        string? warningMessage = null;
        if (_emailService.IsConfigured)
        {
            var emailSent = await _emailService.SendWelcomeAsync(user.Email, user.UserName);
            if (!emailSent)
            {
                warningMessage = LocalizedStrings.Format(
                    "WarningEmailNotSent",
                    _emailService.LastError ?? "неизвестная ошибка");
            }
        }

        CurrentUser = user;
        CurrentUserChanged?.Invoke();
        return ServiceResult<User>.Success(user, warningMessage);
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
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterUserName"));
        }

        if (userName.Trim().Length < 2 || userName.Trim().Length > 50)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorUserNameLength"));
        }

        if (!IsEmailValid(email))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterValidEmail"));
        }

        if (!IsPhoneValid(phone))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEnterValidPhone"));
        }

        if (password.Length < 6)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorPasswordLength"));
        }

        if (password != confirmPassword)
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorPasswordsMismatch"));
        }

        if (await _unitOfWork.Users.IsEmailTakenAsync(email))
        {
            return ServiceResult.Fail(LocalizedStrings.Get("ErrorEmailAlreadyExists"));
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

    public static bool IsPhoneValid(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return false;
        }

        var digits = Regex.Replace(phone.Trim(), @"[\s\-\(\)\+]", "");
        return digits.Length >= 7 && digits.Length <= 15 && Regex.IsMatch(digits, @"^\d+$");
    }
}
