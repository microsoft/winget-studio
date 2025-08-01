// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Models;
public class NumberValue : ConfigurationPropertyValueBase
{
    private double _value;

    public override object Value
    {
        get => _value;
        set => _value = Convert.ToDouble(value);
    }

    public NumberValue(double value) : base(PropertyType.Number)
    {
        _value = value;
    }
}
