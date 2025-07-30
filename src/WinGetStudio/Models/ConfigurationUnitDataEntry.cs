// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetStudio.Models;

public partial class ConfigurationUnitDataEntry : ObservableObject
{
    [ObservableProperty]
    private string _key;

    [ObservableProperty]
    private string _value;
}
