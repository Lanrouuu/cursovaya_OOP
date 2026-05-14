using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class AdvertisementsViewModel : ViewModelBase, IRefreshableViewModel
{
    private readonly AdvertisementService _advertisementService;
    private readonly CategoryService _categoryService;
    private readonly AuthService _authService;
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

    public AdvertisementsViewModel(
        AdvertisementService advertisementService,
        CategoryService categoryService,
        AuthService authService,
        DialogService dialogService,
        Func<Advertisement, Task> openDetails,
        Func<Advertisement?, Task> openAddEdit)
    {
        _advertisementService = advertisementService;
        _categoryService = categoryService;
        _authService = authService;
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
    }

    public ObservableCollection<Advertisement> Advertisements { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<ItemCondition> ConditionValues { get; }
    public ObservableCollection<OptionItem> SortOptions { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ReloadAfterChange();
            }
        }
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                ReloadAfterChange();
            }
        }
    }

    public string MinPriceText
    {
        get => _minPriceText;
        set
        {
            if (SetProperty(ref _minPriceText, value))
            {
                ReloadAfterChange();
            }
        }
    }

    public string MaxPriceText
    {
        get => _maxPriceText;
        set
        {
            if (SetProperty(ref _maxPriceText, value))
            {
                ReloadAfterChange();
            }
        }
    }

    public string City
    {
        get => _city;
        set
        {
            if (SetProperty(ref _city, value))
            {
                ReloadAfterChange();
            }
        }
    }

    public ItemCondition? SelectedCondition
    {
        get => _selectedCondition;
        set
        {
            if (SetProperty(ref _selectedCondition, value))
            {
                ReloadAfterChange();
            }
        }
    }

    public DateTime? DateFrom
    {
        get => _dateFrom;
        set
        {
            if (SetProperty(ref _dateFrom, value))
            {
                ReloadAfterChange();
            }
        }
    }

    public OptionItem SelectedSortOption
    {
        get => _selectedSortOption;
        set
        {
            if (SetProperty(ref _selectedSortOption, value))
            {
                ReloadAfterChange();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoggedIn => _authService.IsLoggedIn;

    public RelayCommand OpenDetailsCommand { get; }
    public RelayCommand AddAdvertisementCommand { get; }
    public RelayCommand EditAdvertisementCommand { get; }
    public RelayCommand DeleteAdvertisementCommand { get; }
    public RelayCommand ApplyFiltersCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }

    public async Task LoadAsync()
    {
        if (_isBusy)
        {
            return;
        }

        try
        {
            _isBusy = true;

            if (Categories.Count == 0)
            {
                Categories.Clear();
                foreach (var category in await _categoryService.GetActiveAsync())
                {
                    Categories.Add(category);
                }
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

            var items = await _advertisementService.GetFilteredAsync(filter, includeInactive: false);
            Advertisements.Clear();
            foreach (var item in items)
            {
                Advertisements.Add(item);
            }

            ErrorMessage = string.Empty;
            _isLoaded = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не удалось загрузить объявления.";
            _dialogService.ShowError($"Проверьте подключение к SQL Server localhost.\n\n{ex.Message}");
        }
        finally
        {
            _isBusy = false;
        }
    }

    public async Task RefreshAsync()
    {
        await LoadAsync();
    }

    private async Task OpenDetailsAsync(object? parameter)
    {
        if (parameter is Advertisement advertisement)
        {
            await _openDetails(advertisement);
        }
    }

    private async Task EditAdvertisementAsync(object? parameter)
    {
        if (parameter is Advertisement advertisement)
        {
            await _openAddEdit(advertisement);
        }
    }

    private async Task DeleteAdvertisementAsync(object? parameter)
    {
        if (parameter is not Advertisement advertisement)
        {
            return;
        }

        if (!_dialogService.Confirm("Удалить выбранное объявление?"))
        {
            return;
        }

        var result = await _advertisementService.DeleteAsync(advertisement.Id, _authService.CurrentUser);
        if (!result.IsSuccess)
        {
            _dialogService.ShowError(result.ErrorMessage);
            return;
        }

        await LoadAsync();
    }

    private bool CanEditAdvertisement(object? parameter)
    {
        if (parameter is not Advertisement advertisement || _authService.CurrentUser == null)
        {
            return false;
        }

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
        {
            await LoadAsync();
        }
    }

    private static decimal? ParsePrice(string value)
    {
        return decimal.TryParse(value, out var price) ? price : null;
    }
}
