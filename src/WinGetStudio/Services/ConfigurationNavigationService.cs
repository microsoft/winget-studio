// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels.ConfigurationFlow;

namespace WinGetStudio.Services;

internal class ConfigurationNavigationService : NavigationService, IConfigurationNavigationService
{
    public ConfigurationNavigationService(IConfigurationPageService pageService)
        : base(pageService)
    {
    }

    public bool NavigateToDefaultPage(object? parameter = null, bool clearNavigation = false)
    {
        return NavigateTo<PreviewFileViewModel>(parameter, clearNavigation);
    }
}
