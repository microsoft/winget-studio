// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using DSCUI.Services.DesiredStateConfiguration.Contracts;

namespace dscui.Models;

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
