// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

public sealed partial class ConfigurationSetPreviewState
{
    public DSCSetViewModel? ActiveSet { get; set; }

    public Tuple<DSCUnitViewModel, DSCUnitViewModel>? SelectedUnit { get; set; }

    public bool IsEditMode { get; set; }

    public bool IsCodeView { get; set; }
}
