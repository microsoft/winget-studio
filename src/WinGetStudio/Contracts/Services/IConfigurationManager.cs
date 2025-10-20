// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.Models;

namespace WinGetStudio.Contracts.Services;

public interface IConfigurationManager
{
    /// <summary>
    /// Gets or sets the active set preview state.
    /// </summary>
    SetPreviewState ActiveSetPreviewState { get; set; }

    /// <summary>
    /// Gets or sets the active set apply state.
    /// </summary>
    SetApplyState ActiveSetApplyState { get; set; }
}
