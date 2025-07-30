using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.ViewModels.ConfigurationFlow;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

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