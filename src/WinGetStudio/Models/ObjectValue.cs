// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using Windows.Foundation.Collections;

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

    public override object ToObject()
    {
        var valueSet = new ValueSet();
        if (_value != null)
        {
            foreach (var entry in _value)
            {
                valueSet.TryAdd(entry.Name, entry.Value.ToObject());
            }
        }

        return valueSet;
    }
}
