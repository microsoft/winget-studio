// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;

namespace WinGetStudio.Services;

internal sealed partial class ConfigurationSetManager : IConfigurationSetManager
{
    public ConfigurationSetPreviewState ActivePreviewState { get; set; }

    public ConfigurationSetManager()
    {
        ActivePreviewState = new();
    }
}
