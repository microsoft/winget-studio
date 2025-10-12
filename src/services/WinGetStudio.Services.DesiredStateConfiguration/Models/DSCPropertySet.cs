// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;
using Windows.Foundation.Collections;
using YamlDotNet.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public sealed partial class DSCPropertySet : Dictionary<string, object>
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public DSCPropertySet()
        : this(null)
    {
    }

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
        var yaml = ToYaml();
        return FromJsonOrYaml(yaml);
    }

    public string ToJson() => JsonSerializer.Serialize(this, _jsonOptions);

    public string ToYaml() => new SerializerBuilder()
        .WithQuotingNecessaryStrings()
        .Build()
        .Serialize(this);

    public static DSCPropertySet FromJsonOrYaml(string input) => new DeserializerBuilder()
        .WithAttemptingUnquotedStringTypeDeserialization()
        .Build()
        .Deserialize<DSCPropertySet>(input);
}
