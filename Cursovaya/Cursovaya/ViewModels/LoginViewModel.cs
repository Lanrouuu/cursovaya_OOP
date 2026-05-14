using Cursovaya.Services;

namespace Cursovaya.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly DialogService _dialogService;
    private readonly Func<Task> _afterLogin;
    private readonly Action _openRegister;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

    public LoginViewModel(
        AuthService authService,
        DialogService dialogService,
        Func<Task> afterLogin,
        Action openRegister)
    {
        _authService = authService;
        _dialogService = dialogService;
        _afterLogin = afterLogin;
        _openRegister = openRegister;

        LoginCommand = new RelayCommand(async _ => await LoginAsync());
        OpenRegisterCommand = new RelayCommand(_ => _openRegister());
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public RelayCommand LoginCommand { get; }
    public RelayCommand OpenRegisterCommand { get; }

    private async Task LoginAsync()
    {
        try
        {
            var result = await _authService.LoginAsync(Email, Password);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            ErrorMessage = string.Empty;
            await _afterLogin();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Не удалось войти: {ex.Message}");
        }
    }
}
