using Cursovaya.Models;

namespace Cursovaya.ViewModels;

public class AdvertisementCardViewModel : ViewModelBase
{
    private bool _isFavorite;
    private int _favoritesCount;

    public AdvertisementCardViewModel(Advertisement advertisement, bool isFavorite, Func<AdvertisementCardViewModel, Task> toggleFavorite)
    {
        Advertisement = advertisement;
        _isFavorite = isFavorite;
        _favoritesCount = advertisement.FavoritesCount;
        ToggleFavoriteCommand = new RelayCommand(async _ => await toggleFavorite(this));
    }

    public Advertisement Advertisement { get; }

    public string Title => Advertisement.Title;
    public string ShortDescription => Advertisement.ShortDescription;
    public string City => Advertisement.City;
    public string SellerName => Advertisement.User?.UserName ?? "";
    public string CategoryName => Advertisement.Category?.Name ?? "";
    public int ViewCount => Advertisement.ViewCount;
    public ItemCondition Condition => Advertisement.Condition;

    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetProperty(ref _isFavorite, value);
    }

    public int FavoritesCount
    {
        get => _favoritesCount;
        set => SetProperty(ref _favoritesCount, value);
    }

    public RelayCommand ToggleFavoriteCommand { get; }
}
