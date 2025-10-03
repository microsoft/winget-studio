// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using WinGetStudio.Models.ResourceProperties;

namespace WinGetStudio.Models;

public sealed partial class ConfigurationProperty : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ConfigurationPropertyValueBase Value { get; set; }

    public ConfigurationProperty(string name, ConfigurationPropertyValueBase value)
    {
        Name = name;
        Value = value;
    }
}
