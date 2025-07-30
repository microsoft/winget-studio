// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;

namespace WinGetStudio.Models;
public class StringValue : ConfigurationPropertyValueBase
{
    private string _value = "";
    public override object Value
    {
        get => _value;
        set => _value = (string)value;
    }
    public StringValue(string value) : base(PropertyType.String)
    {
        _value = value;
    }
}
