// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;

namespace WinGetStudio.Models;

public class NumberValue : ConfigurationPropertyValueBase
{
    private double _value;

    public override object Value
    {
        get => _value;
        set => _value = Convert.ToDouble(value, CultureInfo.InvariantCulture);
    }

    public NumberValue(double value)
        : base(PropertyType.NumberType)
    {
        _value = value;
    }
}
