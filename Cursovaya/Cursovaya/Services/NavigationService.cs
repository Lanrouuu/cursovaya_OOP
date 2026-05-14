using Cursovaya.ViewModels;

namespace Cursovaya.Services;

public class NavigationService
{
    public ViewModelBase? CurrentViewModel { get; private set; }

    public event Action<ViewModelBase?>? CurrentViewModelChanged;

    public void Navigate(ViewModelBase viewModel)
    {
        CurrentViewModel = viewModel;
        CurrentViewModelChanged?.Invoke(CurrentViewModel);
    }
}
