// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

public sealed partial class SetPreviewState
{
    public SetViewModel? ActiveSet { get; set; }

    public Tuple<UnitViewModel, UnitViewModel>? SelectedUnit { get; set; }

    public bool IsEditMode { get; set; }

    public bool IsCodeView { get; set; }
}
