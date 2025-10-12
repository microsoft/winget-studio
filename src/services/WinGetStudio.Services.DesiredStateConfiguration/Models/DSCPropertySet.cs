// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Windows.Foundation.Collections;
using YamlDotNet.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public sealed partial class DSCPropertySet : Dictionary<string, object>
{
    public DSCPropertySet(ValueSet valueSet = null)
        : base(valueSet ?? [])
    {
    }

    /// <summary>
    /// Deep copy the property set.
    /// </summary>
    /// <returns>A deep copy of the property set.</returns>
    public DSCPropertySet DeepCopy()
    {
        var yaml = new SerializerBuilder()
            .WithQuotingNecessaryStrings()
            .Build()
            .Serialize(this);

        return new DeserializerBuilder()
            .WithAttemptingUnquotedStringTypeDeserialization()
            .Build()
            .Deserialize<DSCPropertySet>(yaml);
    }
}
