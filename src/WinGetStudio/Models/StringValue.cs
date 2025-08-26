// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Models;

public class StringValue : ConfigurationPropertyValueBase
{
    private string _value = string.Empty;

    public override object Value
    {
        get => _value;
        set => _value = (string)value;
    }

    public StringValue(string value)
        : base(PropertyType.StringType)
    {
        _value = value;
    }
}
