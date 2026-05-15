using Cursovaya.Models;
using Cursovaya.Repositories;
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
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult<User>.Fail("Введите email и пароль.");
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null || !PasswordService.VerifyPassword(password, user.PasswordHash))
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
        var validation = await ValidateRegistrationAsync(userName, email, phone, password, confirmPassword);
        if (!validation.IsSuccess)
        {
            return ServiceResult<User>.Fail(validation.ErrorMessage);
        }

        var user = new User
        {
            UserName = userName.Trim(),
            Email = email.Trim(),
            PhoneNumber = phone.Trim(),
            PasswordHash = PasswordService.HashPassword(password),
            Role = UserRole.User,
            CreatedAt = DateTime.Now
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _ = _emailService.SendWelcomeAsync(user.Email, user.UserName);

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

        if (userName.Trim().Length < 2 || userName.Trim().Length > 50)
        {
            return ServiceResult.Fail("Имя пользователя должно содержать от 2 до 50 символов.");
        }

        if (!IsEmailValid(email))
        {
            return ServiceResult.Fail("Введите корректный email.");
        }

        if (!IsPhoneValid(phone))
        {
            return ServiceResult.Fail("Введите корректный номер телефона (минимум 7 цифр).");
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
