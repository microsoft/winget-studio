// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using dscui.Contracts.Services;

namespace dscui.ViewModels;

public partial class ConfigurationViewModel : ObservableRecipient
{
    public IConfigurationNavigationService NavigationService { get; }

    public ConfigurationViewModel(IConfigurationNavigationService navigationService)
    {
        NavigationService = navigationService;
    }
}
