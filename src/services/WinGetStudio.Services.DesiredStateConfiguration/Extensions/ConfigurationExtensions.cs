// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using WinGetStudio.Services.DesiredStateConfiguration.Models.Schemas.ConfigurationV3;
using YamlDotNet.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Extensions;

public static class ConfigurationExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    private const string WinGetMetadataKey = "winget";
    private const string ProcessorMetadataKey = "processor";
    private const string IdentifierMetadataKey = "identifier";
    private const string DSCv3MetadataValue = "dscv3";

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

    public static void ToJson(this ConfigurationV3 config) => ConfigurationToJson(config);

    public static void ToYaml(this ConfigurationV3 config) => ConfigurationToYaml(config);

    private static string ConfigurationToJson<T>(T config) => JsonSerializer.Serialize(config, _jsonOptions);

    private static string ConfigurationToYaml<T>(T config)
    {
        // 1. Serialize to JSON
        var json = ConfigurationToJson(config);

        // 2. Parse JSON as YAML (JSON is a subset of YAML)
        var intermediate = new DeserializerBuilder()
            .WithAttemptingUnquotedStringTypeDeserialization()
            .Build()
            .Deserialize<object>(json);

        // 3. Serialize as YAML
        return new SerializerBuilder()
            .WithQuotingNecessaryStrings()
            .DisableAliases()
            .Build()
            .Serialize(intermediate);
    }
}
