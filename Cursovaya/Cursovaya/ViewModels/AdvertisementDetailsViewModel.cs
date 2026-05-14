using Cursovaya.Models;

namespace Cursovaya.ViewModels;

public class AdvertisementDetailsViewModel : ViewModelBase
{
    private readonly Func<Task> _goBack;
    private bool _showContacts;

    public AdvertisementDetailsViewModel(Advertisement advertisement, Func<Task> goBack)
    {
        Advertisement = advertisement;
        _goBack = goBack;

        ShowContactsCommand = new RelayCommand(_ => ShowContacts = true);
        BackCommand = new RelayCommand(async _ => await _goBack());
    }

    public Advertisement Advertisement { get; }

    public bool ShowContacts
    {
        get => _showContacts;
        set => SetProperty(ref _showContacts, value);
    }

    public RelayCommand ShowContactsCommand { get; }
    public RelayCommand BackCommand { get; }
}
