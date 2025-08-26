// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Models;

public partial class SelectFileResult : ObservableObject
{
    [ObservableProperty]
    public partial string? FilePath { get; set; }

    [ObservableProperty]
    public partial bool Success { get; set; }

    [ObservableProperty]
    public partial string? Message { get; set; }

    [ObservableProperty]
    public partial IDSCSet? ConfigurationSet { get; set; }
}
