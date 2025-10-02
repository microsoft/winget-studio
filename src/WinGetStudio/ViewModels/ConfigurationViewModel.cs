// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Contracts.ViewModels;
using WinGetStudio.ViewModels.ConfigurationFlow;

namespace WinGetStudio.ViewModels;

public partial class ConfigurationViewModel : ObservableRecipient, INavigationAware
{
    public IConfigurationFrameNavigationService NavigationService { get; }

    public ConfigurationViewModel(IConfigurationFrameNavigationService navigationService)
    {
        NavigationService = navigationService;
    }

    public void OnNavigatedTo(object parameter)
    {
        NavigationService.NavigateTo<PreviewFileViewModel>(parameter);
    }

    public void OnNavigatedFrom()
    {
        // No-op
    }
}
