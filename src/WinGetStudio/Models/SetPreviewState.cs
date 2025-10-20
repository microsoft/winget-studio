// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

/// <summary>
/// Represents the state of the current set preview.
/// </summary>
public sealed partial class SetPreviewState
{
    /// <summary>
    /// Gets or sets the active set.
    /// </summary>
    public SetViewModel? ActiveSet { get; set; }

    /// <summary>
    /// Gets or sets the selected unit tuple.
    /// </summary>
    public Tuple<UnitViewModel, UnitViewModel>? SelectedUnit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the preview is in edit mode.
    /// </summary>
    public bool IsEditMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the preview is in code view mode.
    /// </summary>
    public bool IsCodeView { get; set; }
}
