// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using dscui.Contracts.Services;
using dscui.ViewModels.ConfigurationFlow;

namespace dscui.Services;

internal class ConfigurationNavigationService : NavigationService, IConfigurationNavigationService
{
    public ConfigurationNavigationService(IConfigurationPageService pageService)
        : base(pageService)
    {
    }

    public bool NavigateToDefaultPage(object? parameter = null, bool clearNavigation = false)
    {
        return NavigateTo<SelectFileViewModel>(parameter, clearNavigation);
    }
}
