// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;
using YamlDotNet.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Extensions;

public static class ConfigurationExtensions
{
    private const string WinGetMetadataKey = "winget";
    private const string ProcessorMetadataKey = "processor";
    private const string IdentifierMetadataKey = "identifier";
    private const string DSCv3MetadataValue = "dscv3";

    /// <summary>
    /// Add WinGet metadata to the configuration.
    /// </summary>
    /// <param name="config">The configuration to add metadata to.</param>
    public static void AddWinGetMetadata(this ConfigurationV3 config)
    {
        config.Metadata = new Dictionary<string, object>()
        {
            {
                WinGetMetadataKey, new Dictionary<string, object>()
                {
                    {
                        ProcessorMetadataKey, new Dictionary<string, object>()
                        {
                            { IdentifierMetadataKey, DSCv3MetadataValue },
                        }
                    },
                }
            },
        };
    }

    /// <summary>
    /// Converts the configuration to JSON string.
    /// </summary>
    /// <param name="config">The configuration to convert.</param>
    /// <returns>The JSON string.</returns>
    public static string ToJson(this ConfigurationV3 config)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver() { Modifiers = { ConfigurationV3PropertiesOrder } },
        };

        return ConfigurationToJson(config, jsonOptions);
    }

    /// <summary>
    /// Converts the configuration to YAML string.
    /// </summary>
    /// <param name="config">The configuration to convert.</param>
    /// <returns>The YAML string.</returns>
    public static string ToYaml(this ConfigurationV3 config)
    {
        var json = config.ToJson();
        return ConfigurationToYaml(json);
    }

    /// <summary>
    /// Converts any configuration object to JSON string.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <param name="config">The configuration to convert.</param>
    /// <param name="options">The JSON serializer options.</param>
    /// <returns>The JSON string.</returns>
    private static string ConfigurationToJson<T>(T config, JsonSerializerOptions options)
    {
        return JsonSerializer.Serialize(config, options);
    }

    /// <summary>
    /// Converts JSON string to YAML string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The YAML string.</returns>
    private static string ConfigurationToYaml(string json)
    {
        // Parse JSON as YAML (JSON is a subset of YAML)
        var intermediate = new DeserializerBuilder()
            .WithAttemptingUnquotedStringTypeDeserialization()
            .Build()
            .Deserialize<object>(json);

        // Serialize as YAML
        return new SerializerBuilder()
            .WithQuotingNecessaryStrings()
            .DisableAliases()
            .Build()
            .Serialize(intermediate);
    }

    /// <summary>
    /// Updates the property order for the configuration when serializing to JSON.
    /// </summary>
    /// <param name="typeInfo">The type info.</param>
    private static void ConfigurationV3PropertiesOrder(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type == typeof(ConfigurationV3))
        {
            foreach (var p in typeInfo.Properties)
            {
                // Order properties:
                // - $schema
                // - metadata
                // - resources
                if (p.Name == "$schema")
                {
                    p.Order = -3;
                }
                else if (string.Equals(p.Name, nameof(ConfigurationV3.Metadata), StringComparison.OrdinalIgnoreCase))
                {
                    p.Order = -2;
                }
                else if (string.Equals(p.Name, nameof(ConfigurationV3.Resources), StringComparison.OrdinalIgnoreCase))
                {
                    p.Order = -1;
                }
            }
        }
    }
}
