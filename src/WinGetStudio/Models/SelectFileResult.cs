// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Models;

public partial class SelectFileResult : ObservableObject
{
    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private bool _success;

    [ObservableProperty]
    private string? _message;

    [ObservableProperty]
    private IDSCSet? _configurationSet;
}
