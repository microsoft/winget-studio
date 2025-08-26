// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;

namespace WinGetStudio.Models;

public class ObjectValue : ConfigurationPropertyValueBase
{
    private ObservableCollection<ConfigurationProperty> _value = new();

    public override object Value
    {
        get => _value;
        set => _value = (ObservableCollection<ConfigurationProperty>)value;
    }

    public ObjectValue(ObservableCollection<ConfigurationProperty> value)
        : base(PropertyType.ObjectType)
    {
        _value = value;
    }
}
