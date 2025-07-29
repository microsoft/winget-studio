// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using dscui.Contracts.Services;
using dscui.ViewModels.ConfigurationFlow;
using dscui.Views.ConfigurationFlow;

namespace dscui.Services;

internal class ConfigurationPageService : PageService, IConfigurationPageService
{
    protected override void ConfigurePages()
    {
        Configure<SelectFileViewModel, SelectFilePage>();
        Configure<PreviewFileViewModel, PreviewFilePage>();
        Configure<ApplyFileViewModel, ApplyFilePage>();
    }
}
