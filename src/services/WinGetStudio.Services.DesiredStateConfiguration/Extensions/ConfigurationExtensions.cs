// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using WinGetStudio.Services.Core.Helpers;
using WinGetStudio.Services.DesiredStateConfiguration.Helpers;
using WinGetStudio.Services.DesiredStateConfiguration.Models;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;
using ConfigurationV3Resource = WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3.Json6;

namespace WinGetStudio.Services.DesiredStateConfiguration.Extensions;

public static class ConfigurationExtensions
{
    private const string MetadataKey = "metadata";
    private const string WinGetMetadataKey = "winget";
    private const string DescriptionMetadataKey = "description";
    private const string ProcessorMetadataKey = "processor";
    private const string IdentifierMetadataKey = "identifier";
    private const string SecurityContextMetadataKey = "securityContext";
    private const string DSCv3MetadataValue = "dscv3";

    /// <summary>
    /// Adds WinGet metadata to the configuration.
    /// </summary>
    /// <param name="config">The configuration to add metadata to.</param>
    public static void AddWinGetMetadata(this ConfigurationV3 config)
    {
        var metadata = config.Metadata as IDictionary<string, object> ?? new Dictionary<string, object>();
        config.Metadata = metadata;
        var wingetMetadata = EnsureObjectDict(metadata, WinGetMetadataKey);
        var processorMetadata = EnsureObjectDict(wingetMetadata, ProcessorMetadataKey);
        processorMetadata[IdentifierMetadataKey] = DSCv3MetadataValue;
    }

    /// <summary>
    /// Adds the security context to the resource.
    /// </summary>
    /// <param name="resource">The resource to add the security context to.</param>
    /// <param name="securityContext">The security context value.</param>
    public static void AddSecurityContext(this ConfigurationV3Resource resource, string securityContext)
    {
        resource.AdditionalProperties ??= new Dictionary<string, object>();
        var metadata = EnsureObjectDict(resource.AdditionalProperties, MetadataKey);
        var wingetMetadata = EnsureObjectDict(metadata, WinGetMetadataKey);
        wingetMetadata[SecurityContextMetadataKey] = securityContext;
    }

    /// <summary>
    /// Adds metadata to the resource.
    /// </summary>
    /// <param name="resource">The resource to add metadata to.</param>
    /// <param name="metadata">The metadata to add.</param>
    public static void AddMetadata(this ConfigurationV3Resource resource, DSCPropertySet metadata)
    {
        resource.AdditionalProperties ??= new Dictionary<string, object>();
        resource.AdditionalProperties[MetadataKey] = metadata;
    }

    /// <summary>
    /// Adds a description to the resource.
    /// </summary>
    /// <param name="resource">The resource to add the description to.</param>
    /// <param name="description">The description text.</param>
    public static void AddDescription(this ConfigurationV3Resource resource, string description)
    {
        resource.AdditionalProperties ??= new Dictionary<string, object>();
        var metadata = EnsureObjectDict(resource.AdditionalProperties, MetadataKey);
        metadata[DescriptionMetadataKey] = description;
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
        var yaml = DSCYamlHelper.JsonToYaml(json);
        return AddYamlHeaderComment(yaml);
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

    /// <summary>
    /// Ensures that the specified key in the dictionary contains a dictionary object.
    /// </summary>
    /// <param name="dict">The parent dictionary.</param>
    /// <param name="key">The key to check.</param>
    /// <returns>The dictionary object.</returns>
    private static IDictionary<string, object> EnsureObjectDict(IDictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var obj) || obj is not IDictionary<string, object> value)
        {
            value = new Dictionary<string, object>();
            dict[key] = value!;
        }

        return value;
    }

    /// <summary>
    /// Adds a YAML header comment indicating the tool and version used to
    /// create the configuration.
    /// </summary>
    /// <param name="yaml">The YAML string.</param>
    /// <returns>The YAML string with the header comment.</returns>
    private static string AddYamlHeaderComment(string yaml)
    {
        var headerComment = $"# Created with WinGet Studio - v{RuntimeHelper.GetAppVersion()}";
        return headerComment + Environment.NewLine + Environment.NewLine + yaml;
    }
}
