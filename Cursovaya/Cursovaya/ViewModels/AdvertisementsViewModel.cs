using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class AdvertisementsViewModel : ViewModelBase, IRefreshableViewModel
{
    private readonly AdvertisementService _advertisementService;
    private readonly CategoryService _categoryService;
    private readonly AuthService _authService;
    private readonly FavoriteService _favoriteService;
    private readonly DialogService _dialogService;
    private readonly Func<Advertisement, Task> _openDetails;
    private readonly Func<Advertisement?, Task> _openAddEdit;
    private bool _isLoaded;
    private bool _isBusy;

    private string _searchText = string.Empty;
    private Category? _selectedCategory;
    private string _minPriceText = string.Empty;
    private string _maxPriceText = string.Empty;
    private string _city = string.Empty;
    private ItemCondition? _selectedCondition;
    private DateTime? _dateFrom;
    private OptionItem _selectedSortOption;
    private string _errorMessage = string.Empty;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalCount;

    private const int PageSize = 10;

    public AdvertisementsViewModel(
        AdvertisementService advertisementService,
        CategoryService categoryService,
        AuthService authService,
        FavoriteService favoriteService,
        DialogService dialogService,
        Func<Advertisement, Task> openDetails,
        Func<Advertisement?, Task> openAddEdit)
    {
        _advertisementService = advertisementService;
        _categoryService = categoryService;
        _authService = authService;
        _favoriteService = favoriteService;
        _dialogService = dialogService;
        _openDetails = openDetails;
        _openAddEdit = openAddEdit;

        SortOptions = new ObservableCollection<OptionItem>
        {
            new() { Title = "Сначала новые", Value = "date_desc" },
            new() { Title = "Цена по возрастанию", Value = "price_asc" },
            new() { Title = "Цена по убыванию", Value = "price_desc" },
            new() { Title = "По названию", Value = "title" }
        };
        _selectedSortOption = SortOptions[0];

        ConditionValues = new ObservableCollection<ItemCondition>(Enum.GetValues<ItemCondition>());

        OpenDetailsCommand = new RelayCommand(async parameter => await OpenDetailsAsync(parameter));
        AddAdvertisementCommand = new RelayCommand(async _ => await _openAddEdit(null), _ => _authService.IsLoggedIn);
        EditAdvertisementCommand = new RelayCommand(async parameter => await EditAdvertisementAsync(parameter), CanEditAdvertisement);
        DeleteAdvertisementCommand = new RelayCommand(async parameter => await DeleteAdvertisementAsync(parameter), CanEditAdvertisement);
        ApplyFiltersCommand = new RelayCommand(async _ => await LoadAsync());
        ClearFiltersCommand = new RelayCommand(async _ => await ClearFiltersAsync());
        NextPageCommand = new RelayCommand(async _ => await GoToPageAsync(CurrentPage + 1), _ => CurrentPage < TotalPages);
        PreviousPageCommand = new RelayCommand(async _ => await GoToPageAsync(CurrentPage - 1), _ => CurrentPage > 1);
    }

    public ObservableCollection<AdvertisementCardViewModel> Advertisements { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<ItemCondition> ConditionValues { get; }
    public ObservableCollection<OptionItem> SortOptions { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ReloadAfterChange();
        }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
                ReloadAfterChange();
        }
    }

    public string MinPriceText
    {
        get => _minPriceText;
        set
        {
            if (SetProperty(ref _minPriceText, value))
                ReloadAfterChange();
        }
    }

    public string MaxPriceText
    {
        get => _maxPriceText;
        set
        {
            if (SetProperty(ref _maxPriceText, value))
                ReloadAfterChange();
        }
    }

    public string City
    {
        get => _city;
        set
        {
            if (SetProperty(ref _city, value))
                ReloadAfterChange();
        }
    }

    public ItemCondition? SelectedCondition
    {
        get => _selectedCondition;
        set
        {
            if (SetProperty(ref _selectedCondition, value))
                ReloadAfterChange();
        }
    }

    public DateTime? DateFrom
    {
        get => _dateFrom;
        set
        {
            if (SetProperty(ref _dateFrom, value))
                ReloadAfterChange();
        }
    }

    public OptionItem SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (SetProperty(ref _selectedSortOption, value))
                ReloadAfterChange();
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (SetProperty(ref _currentPage, value))
            {
                OnPropertyChanged(nameof(PageInfo));
                NextPageCommand.RaiseCanExecuteChanged();
                PreviousPageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        private set
        {
            if (SetProperty(ref _totalPages, value))
            {
                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(ShowPagination));
                NextPageCommand.RaiseCanExecuteChanged();
                PreviousPageCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public string PageInfo => $"{CurrentPage} / {TotalPages}";
    public bool ShowPagination => TotalPages > 1;
    public bool IsLoggedIn => _authService.IsLoggedIn;

    public RelayCommand OpenDetailsCommand { get; }
    public RelayCommand AddAdvertisementCommand { get; }
    public RelayCommand EditAdvertisementCommand { get; }
    public RelayCommand DeleteAdvertisementCommand { get; }
    public RelayCommand ApplyFiltersCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public RelayCommand NextPageCommand { get; }
    public RelayCommand PreviousPageCommand { get; }

    public async Task LoadAsync()
    {
        _currentPage = 1;
        await LoadPageAsync();
    }

    public async Task RefreshAsync()
    {
        await LoadPageAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        _currentPage = page;
        await LoadPageAsync();
    }

    private async Task LoadPageAsync()
    {
        if (_isBusy) return;

        try
        {
            _isBusy = true;

            await _advertisementService.ExpireOverdueAsync();
            await _advertisementService.NotifyExpiringAsync();

            if (Categories.Count == 0)
            {
                Categories.Clear();
                foreach (var category in await _categoryService.GetActiveAsync())
                    Categories.Add(category);
            }

            var filter = new AdvertisementFilter
            {
                SearchText = SearchText,
                CategoryId = SelectedCategory?.Id,
                MinPrice = ParsePrice(MinPriceText),
                MaxPrice = ParsePrice(MaxPriceText),
                City = City,
                Condition = SelectedCondition,
                DateFrom = DateFrom,
                SortMode = SelectedSortOption.Value
            };

            var result = await _advertisementService.GetPagedAsync(filter, includeInactive: false, _currentPage, PageSize);

            HashSet<int> favoriteIds = _authService.IsLoggedIn && _authService.CurrentUser != null
                ? await _favoriteService.GetFavoriteIdsAsync(_authService.CurrentUser.Id)
                : new HashSet<int>();

            Advertisements.Clear();
            foreach (var item in result.Items)
                Advertisements.Add(new AdvertisementCardViewModel(item, favoriteIds.Contains(item.Id), ToggleFavoriteAsync));

            TotalCount = result.TotalCount;
            TotalPages = Math.Max(1, result.TotalPages);
            CurrentPage = result.Page;

            ErrorMessage = string.Empty;
            _isLoaded = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не удалось загрузить объявления.";
            _dialogService.ShowError($"Проверьте подключение к SQL Server.\n\n{ex.Message}");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task ToggleFavoriteAsync(AdvertisementCardViewModel card)
    {
        if (!_authService.IsLoggedIn || _authService.CurrentUser == null)
        {
            _dialogService.ShowError("Войдите в аккаунт, чтобы добавлять в избранное.");
            return;
        }

        var added = await _favoriteService.ToggleAsync(_authService.CurrentUser.Id, card.Advertisement.Id);
        card.IsFavorite = added;
        card.FavoritesCount = added ? card.FavoritesCount + 1 : Math.Max(0, card.FavoritesCount - 1);
    }

    private async Task OpenDetailsAsync(object? parameter)
    {
        if (parameter is Advertisement advertisement)
            await _openDetails(advertisement);
    }

    private async Task EditAdvertisementAsync(object? parameter)
    {
        if (parameter is Advertisement advertisement)
            await _openAddEdit(advertisement);
    }

    private async Task DeleteAdvertisementAsync(object? parameter)
    {
        if (parameter is not Advertisement advertisement) return;

        if (!_dialogService.Confirm("Удалить выбранное объявление?")) return;

        var result = await _advertisementService.DeleteAsync(advertisement.Id, _authService.CurrentUser);
        if (!result.IsSuccess)
        {
            _dialogService.ShowError(result.ErrorMessage);
            return;
        }

        await LoadPageAsync();
    }

    private bool CanEditAdvertisement(object? parameter)
    {
        if (parameter is not Advertisement advertisement || _authService.CurrentUser == null)
            return false;

        return _authService.IsAdmin || advertisement.UserId == _authService.CurrentUser.Id;
    }

    private async Task ClearFiltersAsync()
    {
        SearchText = string.Empty;
        SelectedCategory = null;
        MinPriceText = string.Empty;
        MaxPriceText = string.Empty;
        City = string.Empty;
        SelectedCondition = null;
        DateFrom = null;
        SelectedSortOption = SortOptions[0];
        await LoadAsync();
    }

    private async void ReloadAfterChange()
    {
        if (_isLoaded)
            await LoadAsync();
    }

    private static decimal? ParsePrice(string value)
    {
        return decimal.TryParse(value, out var price) ? price : null;
    }
}
