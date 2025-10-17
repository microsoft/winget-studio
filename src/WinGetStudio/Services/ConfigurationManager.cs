// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;

namespace WinGetStudio.Services;

internal sealed partial class ConfigurationManager : IConfigurationManager
{
    public SetPreviewState ActiveSetPreviewState { get; set; }

    public SetApplyState ActiveSetApplyState { get; set; }

    public ConfigurationManager()
    {
        ActiveSetPreviewState = new();
        ActiveSetApplyState = new();
    }
}
