using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class AdminPanelViewModel : ViewModelBase, IRefreshableViewModel
{
    private readonly AuthService _authService;
    private readonly AdvertisementService _advertisementService;
    private readonly DialogService _dialogService;
    private string _searchText = string.Empty;
    private Advertisement? _selectedAdvertisement;
    private AdvertisementStatus? _selectedStatus;
    private bool _isBusy;

    public AdminPanelViewModel(
        AuthService authService,
        AdvertisementService advertisementService,
        CategoryService categoryService,
        UserService userService,
        DialogService dialogService)
    {
        _authService = authService;
        _advertisementService = advertisementService;
        _dialogService = dialogService;

        UsersViewModel = new UsersManagementViewModel(userService, dialogService);
        CategoriesViewModel = new CategoriesViewModel(categoryService, dialogService);
        StatusValues = new ObservableCollection<AdvertisementStatus>(Enum.GetValues<AdvertisementStatus>());

        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
        SetActiveCommand = new RelayCommand(async _ => await ChangeStatusAsync(AdvertisementStatus.Active), _ => SelectedAdvertisement != null);
        HideCommand = new RelayCommand(async _ => await ChangeStatusAsync(AdvertisementStatus.Hidden), _ => SelectedAdvertisement != null);
        BlockCommand = new RelayCommand(async _ => await ChangeStatusAsync(AdvertisementStatus.Blocked), _ => SelectedAdvertisement != null);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedAdvertisement != null);
        ApplyStatusFilterCommand = new RelayCommand(async _ => await LoadAdvertisementsAsync());
        ClearFilterCommand = new RelayCommand(async _ => await ClearFilterAsync());
    }

    public UsersManagementViewModel UsersViewModel { get; }
    public CategoriesViewModel CategoriesViewModel { get; }
    public ObservableCollection<Advertisement> Advertisements { get; } = new();
    public ObservableCollection<AdvertisementStatus> StatusValues { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ReloadAdvertisementsAfterChange();
            }
        }
    }

    public Advertisement? SelectedAdvertisement
    {
        get => _selectedAdvertisement;
        set
        {
            if (SetProperty(ref _selectedAdvertisement, value))
            {
                RaiseAdvertisementCommandStates();
            }
        }
    }

    public AdvertisementStatus? SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                ReloadAdvertisementsAfterChange();
            }
        }
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand SetActiveCommand { get; }
    public RelayCommand HideCommand { get; }
    public RelayCommand BlockCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand ApplyStatusFilterCommand { get; }
    public RelayCommand ClearFilterCommand { get; }

    public async Task LoadAsync()
    {
        await UsersViewModel.LoadAsync();
        await CategoriesViewModel.LoadAsync();
        await LoadAdvertisementsAsync();
    }

    public async Task RefreshAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAdvertisementsAsync()
    {
        if (_isBusy)
        {
            return;
        }

        try
        {
            _isBusy = true;
            var filter = new AdvertisementFilter
            {
                SearchText = SearchText,
                Status = SelectedStatus,
                SortMode = "date_desc"
            };

            Advertisements.Clear();
            foreach (var advertisement in await _advertisementService.GetFilteredAsync(filter, includeInactive: true))
            {
                Advertisements.Add(advertisement);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Не удалось загрузить объявления админ-панели: {ex.Message}");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task ChangeStatusAsync(AdvertisementStatus status)
    {
        if (SelectedAdvertisement == null)
        {
            return;
        }

        var result = await _advertisementService.ChangeStatusAsync(SelectedAdvertisement.Id, status, _authService.CurrentUser);
        if (!result.IsSuccess)
        {
            _dialogService.ShowError(result.ErrorMessage);
            return;
        }

        await LoadAdvertisementsAsync();
    }

    private async Task DeleteAsync()
    {
        if (SelectedAdvertisement == null)
        {
            return;
        }

        if (!_dialogService.Confirm("Удалить объявление из админ-панели?"))
        {
            return;
        }

        var result = await _advertisementService.DeleteAsync(SelectedAdvertisement.Id, _authService.CurrentUser);
        if (!result.IsSuccess)
        {
            _dialogService.ShowError(result.ErrorMessage);
            return;
        }

        await LoadAdvertisementsAsync();
    }

    private async Task ClearFilterAsync()
    {
        SearchText = string.Empty;
        SelectedStatus = null;
        await LoadAdvertisementsAsync();
    }

    private async void ReloadAdvertisementsAfterChange()
    {
        await LoadAdvertisementsAsync();
    }

    private void RaiseAdvertisementCommandStates()
    {
        SetActiveCommand.RaiseCanExecuteChanged();
        HideCommand.RaiseCanExecuteChanged();
        BlockCommand.RaiseCanExecuteChanged();
        DeleteCommand.RaiseCanExecuteChanged();
    }
}
