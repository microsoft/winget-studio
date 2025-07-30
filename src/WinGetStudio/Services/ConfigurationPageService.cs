// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels;
using WinGetStudio.ViewModels.ConfigurationFlow;
using WinGetStudio.Views;
using WinGetStudio.Views.ConfigurationFlow;

namespace WinGetStudio.Services;

internal class ConfigurationPageService : PageService, IConfigurationPageService
{
    protected override void ConfigurePages()
    {
        Configure<PreviewFileViewModel, PreviewFilePage>();
        Configure<ApplyFileViewModel, ApplyFilePage>();
    }
}
