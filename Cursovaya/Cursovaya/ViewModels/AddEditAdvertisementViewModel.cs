using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class AddEditAdvertisementViewModel : ViewModelBase
{
    private readonly Advertisement? _editingAdvertisement;
    private readonly AuthService _authService;
    private readonly AdvertisementService _advertisementService;
    private readonly CategoryService _categoryService;
    private readonly DialogService _dialogService;
    private readonly ImageService _imageService;
    private readonly Func<Task> _afterSave;

    private string _title = string.Empty;
    private string _shortDescription = string.Empty;
    private string _fullDescription = string.Empty;
    private string _priceText = "0";
    private string _city = string.Empty;
    private ItemCondition _condition = ItemCondition.Used;
    private Category? _selectedCategory;
    private string _imagePath = string.Empty;
    private string _sellerContactEmail = string.Empty;
    private string _sellerContactPhone = string.Empty;
    private string _errorMessage = string.Empty;

    public AddEditAdvertisementViewModel(
        Advertisement? editingAdvertisement,
        AuthService authService,
        AdvertisementService advertisementService,
        CategoryService categoryService,
        DialogService dialogService,
        ImageService imageService,
        Func<Task> afterSave)
    {
        _editingAdvertisement = editingAdvertisement;
        _authService = authService;
        _advertisementService = advertisementService;
        _categoryService = categoryService;
        _dialogService = dialogService;
        _imageService = imageService;
        _afterSave = afterSave;

        ConditionValues = new ObservableCollection<ItemCondition>(Enum.GetValues<ItemCondition>());
        SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave);
        CancelCommand = new RelayCommand(async _ => await _afterSave());
        BrowseImageCommand = new RelayCommand(_ => BrowseImage());

        if (_editingAdvertisement != null)
        {
            FillFromAdvertisement(_editingAdvertisement);
        }
        else if (_authService.CurrentUser != null)
        {
            SellerContactEmail = _authService.CurrentUser.Email;
            SellerContactPhone = _authService.CurrentUser.PhoneNumber;
        }
    }

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<ItemCondition> ConditionValues { get; }
    public bool IsEditMode => _editingAdvertisement != null;
    public string PageTitle => IsEditMode ? "Редактирование объявления" : "Новое объявление";

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                OnInputChanged();
            }
        }
    }

    public string ShortDescription
    {
        get => _shortDescription;
        set
        {
            if (SetProperty(ref _shortDescription, value))
            {
                OnInputChanged();
            }
        }
    }

    public string FullDescription
    {
        get => _fullDescription;
        set
        {
            if (SetProperty(ref _fullDescription, value))
            {
                OnInputChanged();
            }
        }
    }

    public string PriceText
    {
        get => _priceText;
        set
        {
            if (SetProperty(ref _priceText, value))
            {
                OnInputChanged();
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
                OnInputChanged();
            }
        }
    }

    public ItemCondition Condition
    {
        get => _condition;
        set => SetProperty(ref _condition, value);
    }

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                OnInputChanged();
            }
        }
    }

    public string ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }

    public string SellerContactEmail
    {
        get => _sellerContactEmail;
        set
        {
            if (SetProperty(ref _sellerContactEmail, value))
            {
                OnInputChanged();
            }
        }
    }

    public string SellerContactPhone
    {
        get => _sellerContactPhone;
        set
        {
            if (SetProperty(ref _sellerContactPhone, value))
            {
                OnInputChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(Title) &&
        !string.IsNullOrWhiteSpace(ShortDescription) &&
        !string.IsNullOrWhiteSpace(FullDescription) &&
        decimal.TryParse(PriceText, out var price) &&
        price >= 0 &&
        SelectedCategory != null &&
        !string.IsNullOrWhiteSpace(City) &&
        AuthService.IsEmailValid(SellerContactEmail) &&
        !string.IsNullOrWhiteSpace(SellerContactPhone);

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand BrowseImageCommand { get; }

    public async Task LoadAsync()
    {
        try
        {
            Categories.Clear();
            foreach (var category in await _categoryService.GetActiveAsync())
            {
                Categories.Add(category);
            }

            if (_editingAdvertisement != null)
            {
                SelectedCategory = Categories.FirstOrDefault(x => x.Id == _editingAdvertisement.CategoryId);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Не удалось загрузить категории: {ex.Message}");
        }
    }

    private async Task SaveAsync()
    {
        if (!ValidateForSave(out var price))
        {
            return;
        }

        var advertisement = new Advertisement
        {
            Id = _editingAdvertisement?.Id ?? 0,
            Title = Title,
            ShortDescription = ShortDescription,
            FullDescription = FullDescription,
            Price = price,
            City = City,
            Condition = Condition,
            CategoryId = SelectedCategory!.Id,
            ImagePath = ImagePath,
            SellerContactEmail = SellerContactEmail,
            SellerContactPhone = SellerContactPhone
        };

        try
        {
            ServiceResult result = IsEditMode
                ? await _advertisementService.UpdateAsync(advertisement, _authService.CurrentUser)
                : await _advertisementService.AddAsync(advertisement, _authService.CurrentUser);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            ErrorMessage = string.Empty;
            await _afterSave();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Не удалось сохранить объявление: {ex.Message}");
        }
    }

    private bool ValidateForSave(out decimal price)
    {
        price = 0;

        if (!decimal.TryParse(PriceText, out price) || price < 0)
        {
            ErrorMessage = "Цена должна быть числом не меньше 0.";
            return false;
        }

        if (!CanSave)
        {
            ErrorMessage = "Заполните все обязательные поля.";
            return false;
        }

        return true;
    }

    private void BrowseImage()
    {
        var selectedPath = _dialogService.SelectImagePath();
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        ImagePath = _imageService.CopyToUserImages(selectedPath);
    }

    private void FillFromAdvertisement(Advertisement advertisement)
    {
        Title = advertisement.Title;
        ShortDescription = advertisement.ShortDescription;
        FullDescription = advertisement.FullDescription;
        PriceText = advertisement.Price.ToString("0.##");
        City = advertisement.City;
        Condition = advertisement.Condition;
        ImagePath = advertisement.ImagePath;
        SellerContactEmail = advertisement.SellerContactEmail;
        SellerContactPhone = advertisement.SellerContactPhone;
    }

    private void OnInputChanged()
    {
        OnPropertyChanged(nameof(CanSave));
        SaveCommand.RaiseCanExecuteChanged();
    }
}
