// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Models;
public class BooleanValue : ConfigurationPropertyValueBase
{
    private bool _value;

    public override object Value
    {
        get => _value;
        set => _value = (bool)value;
    }
    public BooleanValue(bool value) : base(PropertyType.Boolean)
    {
        _value = value;
    }
}
