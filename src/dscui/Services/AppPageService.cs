// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using dscui.Contracts.Services;
using dscui.ViewModels;
using dscui.Views;

namespace dscui.Services;

internal class AppPageService : PageService, IAppPageService
{
    protected override void ConfigurePages()
    {
        Configure<MainViewModel, MainPage>();
        Configure<ConfigurationViewModel, ConfigurationPage>();
        Configure<SettingsViewModel, SettingsPage>();
    }
}
