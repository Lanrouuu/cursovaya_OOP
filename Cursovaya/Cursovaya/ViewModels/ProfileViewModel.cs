using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class ProfileViewModel : ViewModelBase, IRefreshableViewModel
{
    private readonly AuthService _authService;
    private readonly UserService _userService;
    private readonly AdvertisementService _advertisementService;
    private readonly ThemeService _themeService;
    private readonly LocalizationService _localizationService;
    private readonly DialogService _dialogService;
    private readonly Func<Advertisement?, Task> _openEdit;

    private string _userName = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _selectedTheme;
    private OptionItem _selectedLanguage;
    private string _errorMessage = string.Empty;

    public ProfileViewModel(
        AuthService authService,
        UserService userService,
        AdvertisementService advertisementService,
        ThemeService themeService,
        LocalizationService localizationService,
        DialogService dialogService,
        Func<Advertisement?, Task> openEdit)
    {
        _authService = authService;
        _userService = userService;
        _advertisementService = advertisementService;
        _themeService = themeService;
        _localizationService = localizationService;
        _dialogService = dialogService;
        _openEdit = openEdit;
        _selectedTheme = _themeService.CurrentTheme;
        Themes = new ObservableCollection<string>(_themeService.Themes);
        Languages = new ObservableCollection<OptionItem>
        {
            new() { Title = "RU", Value = "ru-RU" },
            new() { Title = "EN", Value = "en-US" }
        };
        _selectedLanguage = Languages.First(x => x.Value == _localizationService.CurrentLanguage);

        SaveProfileCommand = new RelayCommand(async _ => await SaveProfileAsync());
        EditAdvertisementCommand = new RelayCommand(async parameter => await EditAdvertisementAsync(parameter));
    }

    public ObservableCollection<Advertisement> MyAdvertisements { get; } = new();
    public ObservableCollection<string> Themes { get; }
    public ObservableCollection<OptionItem> Languages { get; }

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

    public string SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value))
            {
                _themeService.ApplyTheme(value);
            }
        }
    }

    public OptionItem SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value))
            {
                _localizationService.ApplyLanguage(value.Value);
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public RelayCommand SaveProfileCommand { get; }
    public RelayCommand EditAdvertisementCommand { get; }

    public async Task LoadAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            return;
        }

        UserName = currentUser.UserName;
        Email = currentUser.Email;
        PhoneNumber = currentUser.PhoneNumber;

        await LoadAdvertisementsAsync();
    }

    public async Task RefreshAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAdvertisementsAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            return;
        }

        try
        {
            MyAdvertisements.Clear();
            foreach (var item in await _advertisementService.GetByUserAsync(currentUser.Id))
            {
                MyAdvertisements.Add(item);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Не удалось загрузить личные объявления: {ex.Message}");
        }
    }

    private async Task SaveProfileAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            return;
        }

        var result = await _userService.UpdateProfileAsync(currentUser, UserName, Email, PhoneNumber);
        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        ErrorMessage = string.Empty;
        _dialogService.ShowMessage("Профиль сохранён.");
    }

    private async Task EditAdvertisementAsync(object? parameter)
    {
        if (parameter is Advertisement advertisement)
        {
            await _openEdit(advertisement);
        }
    }
}
