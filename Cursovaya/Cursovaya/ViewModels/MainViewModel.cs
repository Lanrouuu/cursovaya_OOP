using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly AdvertisementService _advertisementService;
    private readonly CategoryService _categoryService;
    private readonly UserService _userService;
    private readonly DialogService _dialogService;
    private readonly ThemeService _themeService;
    private readonly LocalizationService _localizationService;
    private readonly ImageService _imageService;
    private readonly UndoRedoService _undoRedoService;
    private readonly NavigationService _navigationService;

    private ViewModelBase? _currentViewModel;
    private string _selectedTheme;
    private OptionItem _selectedLanguage;

    public MainViewModel(
        AuthService authService,
        AdvertisementService advertisementService,
        CategoryService categoryService,
        UserService userService,
        DialogService dialogService,
        ThemeService themeService,
        LocalizationService localizationService,
        ImageService imageService,
        UndoRedoService undoRedoService,
        NavigationService navigationService)
    {
        _authService = authService;
        _advertisementService = advertisementService;
        _categoryService = categoryService;
        _userService = userService;
        _dialogService = dialogService;
        _themeService = themeService;
        _localizationService = localizationService;
        _imageService = imageService;
        _undoRedoService = undoRedoService;
        _navigationService = navigationService;
        _selectedTheme = _themeService.CurrentTheme;
        Themes = new ObservableCollection<string>(_themeService.Themes);
        Languages = new ObservableCollection<OptionItem>
        {
            new() { Title = "RU", Value = "ru-RU" },
            new() { Title = "EN", Value = "en-US" }
        };
        _selectedLanguage = Languages[0];

        NavigateAdvertisementsCommand = new RelayCommand(async _ => await ShowAdvertisementsAsync());
        NavigateLoginCommand = new RelayCommand(_ => ShowLogin());
        NavigateRegisterCommand = new RelayCommand(_ => ShowRegister());
        NavigateProfileCommand = new RelayCommand(async _ => await ShowProfileAsync(), _ => IsLoggedIn);
        NavigateAdminCommand = new RelayCommand(async _ => await ShowAdminAsync(), _ => IsAdmin);
        LogoutCommand = new RelayCommand(async _ => await LogoutAsync(), _ => IsLoggedIn);
        UndoCommand = new RelayCommand(async _ => await UndoAsync(), _ => CanUndo);
        RedoCommand = new RelayCommand(async _ => await RedoAsync(), _ => CanRedo);

        _authService.CurrentUserChanged += OnAuthChanged;
        _undoRedoService.StateChanged += OnUndoRedoChanged;
        _navigationService.CurrentViewModelChanged += viewModel => CurrentViewModel = viewModel;
    }

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public User? CurrentUser => _authService.CurrentUser;
    public string CurrentUserName => CurrentUser?.UserName ?? (_localizationService.CurrentLanguage == "en-US" ? "Guest" : "Гость");
    public bool IsLoggedIn => _authService.IsLoggedIn;
    public bool IsAdmin => _authService.IsAdmin;
    public bool CanUndo => _undoRedoService.CanUndo;
    public bool CanRedo => _undoRedoService.CanRedo;
    public ObservableCollection<string> Themes { get; }
    public ObservableCollection<OptionItem> Languages { get; }

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
                OnPropertyChanged(nameof(CurrentUserName));
            }
        }
    }

    public RelayCommand NavigateAdvertisementsCommand { get; }
    public RelayCommand NavigateLoginCommand { get; }
    public RelayCommand NavigateRegisterCommand { get; }
    public RelayCommand NavigateProfileCommand { get; }
    public RelayCommand NavigateAdminCommand { get; }
    public RelayCommand LogoutCommand { get; }
    public RelayCommand UndoCommand { get; }
    public RelayCommand RedoCommand { get; }

    public async Task ShowAdvertisementsAsync()
    {
        var viewModel = new AdvertisementsViewModel(
            _advertisementService,
            _categoryService,
            _authService,
            _dialogService,
            OpenDetailsAsync,
            OpenAddEditAdvertisementAsync);

        _navigationService.Navigate(viewModel);
        await viewModel.LoadAsync();
    }

    public void ShowLogin()
    {
        var viewModel = new LoginViewModel(_authService, _dialogService, ShowAdvertisementsAsync, ShowRegister);
        _navigationService.Navigate(viewModel);
    }

    public void ShowRegister()
    {
        var viewModel = new RegisterViewModel(_authService, _dialogService, ShowAdvertisementsAsync, ShowLogin);
        _navigationService.Navigate(viewModel);
    }

    public async Task ShowProfileAsync()
    {
        if (!_authService.IsLoggedIn)
        {
            ShowLogin();
            return;
        }

        var viewModel = new ProfileViewModel(
            _authService,
            _userService,
            _advertisementService,
            _themeService,
            _localizationService,
            _dialogService,
            OpenAddEditAdvertisementAsync);

        _navigationService.Navigate(viewModel);
        await viewModel.LoadAsync();
    }

    public async Task ShowAdminAsync()
    {
        if (!_authService.IsAdmin)
        {
            _dialogService.ShowError("Админ-панель доступна только администратору.");
            return;
        }

        var viewModel = new AdminPanelViewModel(
            _authService,
            _advertisementService,
            _categoryService,
            _userService,
            _dialogService);

        _navigationService.Navigate(viewModel);
        await viewModel.LoadAsync();
    }

    private async Task OpenDetailsAsync(Advertisement advertisement)
    {
        var detailedAdvertisement = await _advertisementService.GetByIdAsync(advertisement.Id);
        if (detailedAdvertisement == null)
        {
            _dialogService.ShowError("Объявление не найдено.");
            return;
        }

        var viewModel = new AdvertisementDetailsViewModel(detailedAdvertisement, ShowAdvertisementsAsync);
        _navigationService.Navigate(viewModel);
    }

    private async Task OpenAddEditAdvertisementAsync(Advertisement? advertisement)
    {
        if (!_authService.IsLoggedIn)
        {
            ShowLogin();
            return;
        }

        Advertisement? detailedAdvertisement = null;
        if (advertisement != null)
        {
            detailedAdvertisement = await _advertisementService.GetByIdAsync(advertisement.Id);
        }

        var viewModel = new AddEditAdvertisementViewModel(
            detailedAdvertisement,
            _authService,
            _advertisementService,
            _categoryService,
            _dialogService,
            _imageService,
            ShowAdvertisementsAsync);

        _navigationService.Navigate(viewModel);
        await viewModel.LoadAsync();
    }

    private async Task LogoutAsync()
    {
        _authService.Logout();
        await ShowAdvertisementsAsync();
    }

    private async Task UndoAsync()
    {
        await _undoRedoService.UndoAsync();
        await RefreshCurrentViewAsync();
    }

    private async Task RedoAsync()
    {
        await _undoRedoService.RedoAsync();
        await RefreshCurrentViewAsync();
    }

    private async Task RefreshCurrentViewAsync()
    {
        if (CurrentViewModel is IRefreshableViewModel refreshable)
        {
            await refreshable.RefreshAsync();
        }
    }

    private void OnAuthChanged()
    {
        OnPropertyChanged(nameof(CurrentUser));
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(IsAdmin));
        NavigateProfileCommand.RaiseCanExecuteChanged();
        NavigateAdminCommand.RaiseCanExecuteChanged();
        LogoutCommand.RaiseCanExecuteChanged();
    }

    private void OnUndoRedoChanged()
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        UndoCommand.RaiseCanExecuteChanged();
        RedoCommand.RaiseCanExecuteChanged();
    }
}
