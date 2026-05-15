using Cursovaya.Models;
using Cursovaya.Services;
using System.Collections.ObjectModel;

namespace Cursovaya.ViewModels;

public class AdvertisementDetailsViewModel : ViewModelBase
{
    private readonly Func<Task> _goBack;
    private readonly AdvertisementService _advertisementService;
    private readonly Func<Advertisement, Task> _openDetails;
    private bool _showContacts;

    public AdvertisementDetailsViewModel(
        Advertisement advertisement,
        Func<Task> goBack,
        AdvertisementService advertisementService,
        Func<Advertisement, Task> openDetails)
    {
        Advertisement = advertisement;
        _goBack = goBack;
        _advertisementService = advertisementService;
        _openDetails = openDetails;

        SimilarAdvertisements = new ObservableCollection<Advertisement>();

        ShowContactsCommand = new RelayCommand(_ => ShowContacts = true);
        BackCommand = new RelayCommand(async _ => await _goBack());
        OpenSimilarCommand = new RelayCommand(async param =>
        {
            if (param is Advertisement ad)
                await _openDetails(ad);
        });
    }

    public Advertisement Advertisement { get; }

    public ObservableCollection<Advertisement> SimilarAdvertisements { get; }

    public bool ShowContacts
    {
        get => _showContacts;
        set => SetProperty(ref _showContacts, value);
    }

    public RelayCommand ShowContactsCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand OpenSimilarCommand { get; }

    public async Task LoadSimilarAsync()
    {
        if (Advertisement.CategoryId <= 0) return;

        var similar = await _advertisementService.GetSimilarAsync(Advertisement.CategoryId, Advertisement.Id);
        SimilarAdvertisements.Clear();
        foreach (var ad in similar)
            SimilarAdvertisements.Add(ad);
    }
}
