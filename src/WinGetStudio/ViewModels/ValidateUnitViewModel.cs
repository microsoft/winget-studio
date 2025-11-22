// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetStudio.ViewModels;

public sealed partial class ValidateUnitViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? SearchResourceText { get; set; }

    [ObservableProperty]
    public partial string? OutputText { get; set; }

    [ObservableProperty]
    public partial string? SettingsText { get; set; }
}
