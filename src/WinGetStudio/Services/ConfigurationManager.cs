// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Contracts.Services;
using WinGetStudio.Models;

namespace WinGetStudio.Services;

internal sealed partial class ConfigurationManager : IConfigurationManager
{
    /// <inheritdoc/>
    public SetPreviewState ActiveSetPreviewState { get; set; }

    /// <inheritdoc/>
    public SetApplyState ActiveSetApplyState { get; set; }

    public ConfigurationManager()
    {
        ActiveSetPreviewState = new();
        ActiveSetApplyState = new();
    }
}
