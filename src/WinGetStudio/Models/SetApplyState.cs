// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using WinGetStudio.ViewModels;

namespace WinGetStudio.Models;

/// <summary>
/// Represents the state of the currently applied set.
/// </summary>
public sealed partial class SetApplyState
{
    public ApplySetViewModel? ActiveApplySet { get; set; }
}
