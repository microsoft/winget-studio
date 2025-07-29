using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using dscui.Contracts.Services;
using Windows.Storage;

namespace dscui.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly IAppNavigationService _navigationService;

    public MainViewModel(IAppNavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    public async Task StartConfigurationFlowAsync(IStorageFile file)
    {
        _navigationService.NavigateTo<ConfigurationViewModel>(file);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnNavigateToConfigurationAsync()
    {
        _navigationService.NavigateTo<ConfigurationViewModel>();
        await Task.CompletedTask;
    }
}
