// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetStudio.Models;

public partial class ConfigurationProperty(string name, ConfigurationPropertyValueBase value) : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = name;

    [ObservableProperty]
    public partial ConfigurationPropertyValueBase Value { get; set; } = value;
}
