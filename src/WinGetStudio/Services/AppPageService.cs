// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels;
using WinGetStudio.Views;

namespace WinGetStudio.Services;

internal sealed class AppPageService : PageService, IAppPageService
{
    protected override void ConfigurePages()
    {
        Configure<MainViewModel, MainPage>();
        Configure<ConfigurationViewModel, ConfigurationPage>();
        Configure<ValidationFrameViewModel, ValidationFramePage>();
        Configure<SettingsViewModel, SettingsPage>();
    }
}
