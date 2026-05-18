using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class UsersManagementViewModel : ViewModelBase, IRefreshableViewModel
{
    private readonly UserService _userService;
    private readonly AuthService _authService;
    private readonly DialogService _dialogService;
    private readonly Func<Task> _onChanged;
    private string _searchText = string.Empty;
    private User? _selectedUser;
    private List<User> _allUsers = new();

    public UsersManagementViewModel(
        UserService userService,
        AuthService authService,
        DialogService dialogService,
        Func<Task> onChanged)
    {
        _userService = userService;
        _authService = authService;
        _dialogService = dialogService;
        _onChanged = onChanged;
        BlockCommand = new RelayCommand(async _ => await SetBlockedAsync(true), _ => SelectedUser != null);
        UnblockCommand = new RelayCommand(async _ => await SetBlockedAsync(false), _ => SelectedUser != null);
        RefreshCommand = new RelayCommand(async _ => await LoadAsync());
    }

    public ObservableCollection<User> Users { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    public User? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                BlockCommand.RaiseCanExecuteChanged();
                UnblockCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RelayCommand BlockCommand { get; }
    public RelayCommand UnblockCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        try
        {
            _allUsers = await _userService.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(LocalizedStrings.Format("ErrorLoadUsers", ex.Message));
        }
    }

    public async Task RefreshAsync()
    {
        await LoadAsync();
    }

    private async Task SetBlockedAsync(bool isBlocked)
    {
        if (SelectedUser == null)
        {
            return;
        }

        var result = await _userService.SetBlockedAsync(SelectedUser, isBlocked, _authService.CurrentUser);
        if (!result.IsSuccess)
        {
            _dialogService.ShowError(result.ErrorMessage);
            return;
        }

        await LoadAsync();
        await _onChanged();
    }

    private void ApplyFilter()
    {
        var items = _allUsers.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLower();
            items = items.Where(x =>
                x.UserName.ToLower().Contains(search) ||
                x.Email.ToLower().Contains(search) ||
                x.PhoneNumber.ToLower().Contains(search));
        }

        Users.Clear();
        foreach (var item in items)
        {
            Users.Add(item);
        }
    }
}
