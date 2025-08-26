// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;

namespace WinGetStudio.Models;

public abstract class ConfigurationPropertyValueBase(PropertyType type) : ObservableObject
{
    public PropertyType Type { get; set; } = type;

    public abstract object Value { get; set; }
}
