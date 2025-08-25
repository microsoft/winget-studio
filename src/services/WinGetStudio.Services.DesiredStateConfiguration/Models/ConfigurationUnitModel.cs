// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;
using Windows.Foundation.Collections;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace WinGetStudio.Models;

public class ConfigurationUnitModel
{
    public string Type { get; set; } = string.Empty;

    public ValueSet Settings { get; set; } = new();

    public bool ElevatedRequired { get; set; }

    public bool TestResult { get; set; }

    /// <summary>
    /// Converts the current object to its YAML representation.
    /// </summary>
    /// <remarks>The method serializes the object's properties and settings into a YAML-formatted string using
    /// a camel case naming convention. The resulting YAML string can be used for configuration or data exchange
    /// purposes.</remarks>
    /// <returns>A YAML-formatted string representing the current object.</returns>
    public string ToYaml()
    {
        var resource = new Dictionary<string, object>
        {
            ["properties"] = new Dictionary<string, object>
            {
                ["resources"] = new List<Dictionary<string, object>>
                 {
                     new()
                     {
                        ["resource"] = Type,
                        ["settings"] = Settings,
                     },
                 },
            },
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        string yaml = serializer.Serialize(resource);
        return yaml;
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
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var resource = deserializer.Deserialize<Dictionary<string, object>>(yaml);
        if (resource != null
            && resource.TryGetValue("properties", out var properties)
            && properties is List<object> resourceList
            && resourceList.Count > 0
            && resourceList[0] is Dictionary<object, object> dict)
        {
            Type = dict["resource"] as string;

            Settings = new ValueSet();

            TryLoadHelper(Settings, dict["settings"] as Dictionary<object, object>);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Helper method to recursively load settings from a dictionary into a ValueSet.
    /// <summary>
    /// Recursively populates a <see cref="ValueSet"/> with key-value pairs from a deserialized YAML dictionary.
    /// </summary>
    /// <param name="settings">The <see cref="ValueSet"/> to populate with values from the dictionary.</param>
    /// <param name="dict">The dictionary containing key-value pairs parsed from YAML.</param>
    private void TryLoadHelper(ValueSet settings, Dictionary<object, object> dict)
    {
        foreach (var kvp in dict)
        {
            if (kvp.Key is string)
            {
                if (kvp.Value is string)
                {
                    // COM API returns strings for all values, so we need to convert them to appropriate types
                    // Types will always be string, bool, number, or nested dictionary
                    object result = kvp.Value as string;
                    if (bool.TryParse(kvp.Value as string, out bool b))
                    {
                        result = b;
                    }
                    else if (double.TryParse(kvp.Value as string, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                    {
                        result = d;
                    }

                    settings[kvp.Key as string] = result;
                }
                else if (kvp.Value is Dictionary<object, object> subDict)
                {
                    var subSettings = new ValueSet();
                    TryLoadHelper(subSettings, subDict);
                    settings[kvp.Key as string] = subSettings;
                }
            }
        }
    }
}
