using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class CategoriesViewModel : ViewModelBase, IRefreshableViewModel
{
    private readonly CategoryService _categoryService;
    private readonly DialogService _dialogService;
    private Category? _selectedCategory;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private bool _isActive = true;
    private string _errorMessage = string.Empty;

    public CategoriesViewModel(CategoryService categoryService, DialogService dialogService)
    {
        _categoryService = categoryService;
        _dialogService = dialogService;
        AddCommand = new RelayCommand(async _ => await AddAsync());
        UpdateCommand = new RelayCommand(async _ => await UpdateAsync(), _ => SelectedCategory != null);
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedCategory != null);
        ClearCommand = new RelayCommand(_ => ClearForm());
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
    }

    public ObservableCollection<Category> Categories { get; } = new();

    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                FillForm(value);
                UpdateCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public RelayCommand AddCommand { get; }
    public RelayCommand UpdateCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand ClearCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        try
        {
            Categories.Clear();
            foreach (var category in await _categoryService.GetAllAsync())
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(LocalizedStrings.Format("ErrorLoadCategories", ex.Message));
        }
    }

    public async Task RefreshAsync()
    {
        await LoadAsync();
    }

    private async Task AddAsync()
    {
        var result = await _categoryService.AddAsync(Name, Description);
        await ProcessResultAsync(result);
    }

    private async Task UpdateAsync()
    {
        if (SelectedCategory == null)
        {
            return;
        }

        var category = new Category
        {
            Id = SelectedCategory.Id,
            Name = Name,
            Description = Description,
            IsActive = IsActive
        };

        var result = await _categoryService.UpdateAsync(category);
        await ProcessResultAsync(result);
    }

    private async Task DeleteAsync()
    {
        if (SelectedCategory == null)
        {
            return;
        }

        if (!_dialogService.Confirm(LocalizedStrings.Get("ConfirmDeleteCategory")))
        {
            return;
        }

        var result = await _categoryService.DeleteAsync(SelectedCategory);
        await ProcessResultAsync(result);
    }

    private async Task ProcessResultAsync(ServiceResult result)
    {
        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage;
            return;
        }

        ErrorMessage = string.Empty;
        ClearForm();
        await LoadAsync();
    }

    private void FillForm(Category? category)
    {
        if (category == null)
        {
            return;
        }

        Name = category.Name;
        Description = category.Description;
        IsActive = category.IsActive;
    }

    private void ClearForm()
    {
        SelectedCategory = null;
        Name = string.Empty;
        Description = string.Empty;
        IsActive = true;
    }
}
