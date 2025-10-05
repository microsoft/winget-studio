// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Foundation.Collections;
using YamlDotNet.Serialization;
using V0_1 = WinGetStudio.Services.DesiredStateConfiguration.Models.DSCWinGetConfigurationV0_1;
using V0_2 = WinGetStudio.Services.DesiredStateConfiguration.Models.DSCWinGetConfigurationV0_2;
using V0_3 = WinGetStudio.Services.DesiredStateConfiguration.Models.DSCWinGetConfigurationV0_3;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public class ConfigurationUnitModel
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    public string Type { get; set; } = string.Empty;

    // TODO Deep copy of settings to prevent RPC errors and improve stability.
    public ValueSet Settings { get; set; } = new();

    public bool ElevatedRequired { get; set; }

    public bool TestResult { get; set; }

    public string ToJson()
    {
        var config = ToWinGetConfigurationV0_1();
        return JsonSerializer.Serialize(config, _jsonOptions);
    }

    /// <summary>
    /// Converts the current object to its YAML representation.
    /// </summary>
    /// <remarks>The method serializes the object's properties and settings into a YAML-formatted string using
    /// a camel case naming convention. The resulting YAML string can be used for configuration or data exchange
    /// purposes.</remarks>
    /// <returns>A YAML-formatted string representing the current object.</returns>
    public string ToYaml()
    {
        // Parse to JSON
        var json = ToJson();
        using var reader = new StringReader(json);
        var deserializer = new DeserializerBuilder().Build();
        var obj = deserializer.Deserialize(reader);

        // Serialize to YAML
        var serializer = new SerializerBuilder().Build();
        return serializer.Serialize(obj);
    }

    /// <summary>
    /// Attempts to load and parse the provided YAML string into the current object's properties.
    /// </summary>
    /// <remarks>This method deserializes the provided YAML string into a dictionary structure and extracts
    /// specific properties to populate the object's state. If the YAML structure does not match the expected format, or
    /// if an error occurs during deserialization, the method will fail silently and return <see
    /// langword="false"/>.</remarks>
    /// <param name="yaml">The YAML string to be deserialized and processed. Cannot be null or empty.</param>
    /// <returns><see langword="true"/> if the YAML string was successfully parsed and the object's properties were populated;
    /// otherwise, <see langword="false"/> if an error occurred during parsing or the YAML structure was invalid.</returns>
    public bool TryLoad(string yaml)
    {
        // TODO: WIth WithAttemptingUnquotedStringTypeDeserialization, we can simplify this.
        // And potentially remove the need for a type associated with each proerpty (?)
        // var deserializer = new DeserializerBuilder()
        //    .WithAttemptingUnquotedStringTypeDeserialization()
        //    .Build();
        // var resource = deserializer.Deserialize<Dictionary<string, object>>(yaml);
        return false;
    }

    private V0_1.DSCWinGetConfigurationV0_1 ToWinGetConfigurationV0_1()
    {
        return new()
        {
            Properties = new()
            {
                Resources =
                [
                    new()
                    {
                        Resource = Type,
                        Settings = new Dictionary<string, object>(Settings),
                    },
                ],
            },
        };
    }

    private V0_2.DSCWinGetConfigurationV0_2 ToWinGetConfigurationV0_2()
    {
        return new()
        {
            Properties = new()
            {
                Resources =
                [
                    new()
                    {
                        Resource = Type,
                        Settings = new Dictionary<string, object>(Settings),
                    },
                ],
            },
        };
    }

    private V0_3.DSCWinGetConfigurationV0_3 ToWinGetConfigurationV0_3()
    {
        return new()
        {
            Metadata = new Dictionary<string, object>()
            {
                {
                    "winget", new Dictionary<string, object>()
                    {
                        {
                            "processor", new Dictionary<string, object>()
                            {
                                { "identifier", "dscv3" },
                            }
                        },
                    }
                },
            },
            Resources =
            [
                new()
                {
                    Name = $"{Type}-0",
                    Type = Type,
                },
            ],
        };
    }
}
