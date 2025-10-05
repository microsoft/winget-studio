// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using V0_1 = WinGetStudio.Services.DesiredStateConfiguration.Models.DSCWinGetConfigurationV0_1;
using V0_2 = WinGetStudio.Services.DesiredStateConfiguration.Models.DSCWinGetConfigurationV0_2;
using V0_3 = WinGetStudio.Services.DesiredStateConfiguration.Models.DSCWinGetConfigurationV0_3;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public class ConfigurationUnitModel
{
    public string Type { get; set; } = string.Empty;

    public ValueSet Settings { get; set; } = new();

    public bool ElevatedRequired { get; set; }

    public bool TestResult { get; set; }

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

    public V0_1.DSCWinGetConfigurationV0_1 ToWinGetConfigurationV0_1()
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
                        Settings = Settings == null ? [] : new Dictionary<string, object>(Settings),
                    },
                ],
            },
        };
    }

    public V0_2.DSCWinGetConfigurationV0_2 ToWinGetConfigurationV0_2()
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
                        Settings = Settings == null ? [] : new Dictionary<string, object>(Settings),
                    },
                ],
            },
        };
    }

    public V0_3.DSCWinGetConfigurationV0_3 ToWinGetConfigurationV0_3()
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
                    Properties = Settings == null ? [] : new Dictionary<string, object>(Settings),
                },
            ],
        };
    }
}
