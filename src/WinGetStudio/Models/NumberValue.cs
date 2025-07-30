// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
