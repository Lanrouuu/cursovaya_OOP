using Cursovaya.Services;

namespace Cursovaya.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly DialogService _dialogService;
    private readonly Func<Task> _afterRegister;
    private readonly Action _openLogin;

    private string _userName = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _errorMessage = string.Empty;

    public RegisterViewModel(
        AuthService authService,
        DialogService dialogService,
        Func<Task> afterRegister,
        Action openLogin)
    {
        _authService = authService;
        _dialogService = dialogService;
        _afterRegister = afterRegister;
        _openLogin = openLogin;

        RegisterCommand = new RelayCommand(async _ => await RegisterAsync());
        OpenLoginCommand = new RelayCommand(_ => _openLogin());
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public RelayCommand RegisterCommand { get; }
    public RelayCommand OpenLoginCommand { get; }

    private async Task RegisterAsync()
    {
        try
        {
            var result = await _authService.RegisterAsync(UserName, Email, PhoneNumber, Password, ConfirmPassword);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            ErrorMessage = string.Empty;
            if (!string.IsNullOrWhiteSpace(result.WarningMessage))
            {
                _dialogService.ShowMessage(result.WarningMessage);
            }

            await _afterRegister();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(LocalizedStrings.Format("ErrorRegisterFailed", ex.Message));
        }
    }
}
