using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetStudio.Contracts.Services;
using Windows.Storage;
using Windows.System;

namespace WinGetStudio.ViewModels;

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
    [RelayCommand]
    private async Task OnNavigateToValidationAsync()
    {
        _navigationService.NavigateTo<ValidationFrameViewModel>();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OnLaunchAIAsync()
    {
        await Launcher.LaunchUriAsync(new Uri("vscode://GitHub.Copilot-Chat/chat?mode=agent&referrer=wingetstudio"));
    }
}
