// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels.ConfigurationFlow;

namespace WinGetStudio.Services.Navigation;

internal sealed class ConfigurationFrameNavigationService : FrameNavigationService, IConfigurationFrameNavigationService
{
    public ConfigurationFrameNavigationService(IConfigurationPageService pageService)
        : base(pageService)
    {
    }

    public bool NavigateToDefaultPage(object? parameter = null, bool clearNavigation = false)
    {
        return NavigateTo<PreviewFileViewModel>(parameter, clearNavigation);
    }
}
