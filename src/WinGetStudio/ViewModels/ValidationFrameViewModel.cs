using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;

namespace WinGetStudio.ViewModels;

public partial class ValidationFrameViewModel : ObservableRecipient, INavigationAware
{
    public IValidationNavigationService NavigationService { get; }

    public ValidationFrameViewModel(IValidationNavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    public void OnNavigatedTo(object parameter)
    {
        NavigationService.NavigateTo<ValidationViewModel>(parameter);
    }
    public void OnNavigatedFrom()
    {
        // No-op
    }
}