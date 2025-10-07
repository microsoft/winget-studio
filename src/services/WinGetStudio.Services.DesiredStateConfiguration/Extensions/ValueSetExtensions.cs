// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Windows.Foundation.Collections;
using YamlDotNet.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Extensions;

public static class ValueSetExtensions
{
    public static Dictionary<string, object> DeepCopyViaYaml(this ValueSet valueSet)
    {
        var serializer = new SerializerBuilder().WithQuotingNecessaryStrings().Build();
        var deserializer = new DeserializerBuilder().WithAttemptingUnquotedStringTypeDeserialization().Build();
        var yaml = serializer.Serialize(valueSet);
        return deserializer.Deserialize<Dictionary<string, object>>(yaml);
    }
}
