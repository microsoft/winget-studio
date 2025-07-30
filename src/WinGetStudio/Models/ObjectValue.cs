// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinGetStudio.Models;
public class ObjectValue : ConfigurationPropertyValueBase
{
    private ObservableCollection<ConfigurationProperty> _value = new();
    public override object Value
    {
        get => _value;
        set => _value = (ObservableCollection<ConfigurationProperty>)value;
    }
    public ObjectValue(ObservableCollection<ConfigurationProperty> value) : base(PropertyType.Object)
    {
        _value = value;
    }   
}
