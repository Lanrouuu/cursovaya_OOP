using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class ProfileViewModel : ViewModelBase, IRefreshableViewModel
{
    private readonly AuthService _authService;
    private readonly UserService _userService;
    private readonly AdvertisementService _advertisementService;
    private readonly FavoriteService _favoriteService;
    private readonly DialogService _dialogService;
    private readonly Func<Advertisement?, Task> _openEdit;

    private string _userName = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _errorMessage = string.Empty;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmNewPassword = string.Empty;
    private string _passwordErrorMessage = string.Empty;

    public ProfileViewModel(
        AuthService authService,
        UserService userService,
        AdvertisementService advertisementService,
        FavoriteService favoriteService,
        DialogService dialogService,
        Func<Advertisement?, Task> openEdit)
    {
        _authService = authService;
        _userService = userService;
        _advertisementService = advertisementService;
        _favoriteService = favoriteService;
        _dialogService = dialogService;
        _openEdit = openEdit;

        SaveProfileCommand = new RelayCommand(async _ => await SaveProfileAsync());
        ChangePasswordCommand = new RelayCommand(async _ => await ChangePasswordAsync());
        EditAdvertisementCommand = new RelayCommand(async parameter => await EditAdvertisementAsync(parameter));
        RenewAdvertisementCommand = new RelayCommand(async parameter => await RenewAdvertisementAsync(parameter));
    }

    public ObservableCollection<Advertisement> MyAdvertisements { get; } = new();
    public ObservableCollection<Advertisement> FavoriteAdvertisements { get; } = new();

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

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value);
    }

    public string PasswordErrorMessage
    {
        get => _passwordErrorMessage;
        set => SetProperty(ref _passwordErrorMessage, value);
    }

    public RelayCommand SaveProfileCommand { get; }
    public RelayCommand ChangePasswordCommand { get; }
    public RelayCommand EditAdvertisementCommand { get; }
    public RelayCommand RenewAdvertisementCommand { get; }

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
        await LoadFavoritesAsync();
    }

    public async Task RefreshAsync()
    {
        await LoadAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null) return;

        try
        {
            FavoriteAdvertisements.Clear();
            foreach (var item in await _favoriteService.GetFavoritesAsync(currentUser.Id))
                FavoriteAdvertisements.Add(item);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(LocalizedStrings.Format("ErrorLoadFavorites", ex.Message));
        }
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
            _dialogService.ShowError(LocalizedStrings.Format("ErrorLoadMyAdvertisements", ex.Message));
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
        _dialogService.ShowMessage(LocalizedStrings.Get("MessageProfileSaved"));
    }

    private async Task ChangePasswordAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            return;
        }

        var result = await _userService.ChangePasswordAsync(currentUser, CurrentPassword, NewPassword, ConfirmNewPassword);
        if (!result.IsSuccess)
        {
            PasswordErrorMessage = result.ErrorMessage;
            return;
        }

        PasswordErrorMessage = string.Empty;
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
        _dialogService.ShowMessage(LocalizedStrings.Get("MessagePasswordChanged"));
    }

    private async Task EditAdvertisementAsync(object? parameter)
    {
        if (parameter is Advertisement advertisement)
        {
            await _openEdit(advertisement);
        }
    }

    private async Task RenewAdvertisementAsync(object? parameter)
    {
        if (parameter is not Advertisement advertisement) return;

        var result = await _advertisementService.RenewAsync(advertisement.Id, _authService.CurrentUser);
        if (!result.IsSuccess)
        {
            _dialogService.ShowError(result.ErrorMessage);
            return;
        }

        _dialogService.ShowMessage(LocalizedStrings.Get("MessageAdvertisementRenewed"));
        await LoadAdvertisementsAsync();
    }
}
