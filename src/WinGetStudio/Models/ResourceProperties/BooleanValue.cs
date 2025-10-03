// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Models.ResourceProperties;

public sealed partial class BooleanValue : ConfigurationPropertyValueBase
{
    private bool _value;

    public override object Value
    {
        get => _value;
        set => _value = (bool)value;
    }

    public BooleanValue(bool value)
        : base(PropertyType.BooleanType)
    {
        _value = value;
    }
}
