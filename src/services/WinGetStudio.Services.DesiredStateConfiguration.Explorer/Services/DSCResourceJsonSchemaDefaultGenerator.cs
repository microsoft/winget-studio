// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Helpers;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed partial class DSCResourceJsonSchemaDefaultGenerator : IDSCResourceJsonSchemaDefaultGenerator
{
    private readonly SampleJsonDataGeneratorSettings _settings;
    private readonly HashSet<string> _readOnlyProperties = ["_inDesiredState"];

    public DSCResourceJsonSchemaDefaultGenerator()
    {
        _settings = new()
        {
            GenerateOptionalProperties = true,
        };
    }

    /// <summary>
    /// Generate default YAML from the given JSON schema.
    /// </summary>
    /// <param name="jsonSchema">The JSON schema.</param>
    /// <returns>The generated default YAML.</returns>
    public async Task<string> GenerateDefaultYamlFromSchemaAsync(string jsonSchema)
    {
        var json = await GenerateDefaultJsonFromSchemaAsync(jsonSchema);
        return DSCYamlHelper.JsonToYaml(json);
    }

    /// <summary>
    /// Generate default JSON from the given JSON schema.
    /// </summary>
    /// <param name="jsonSchema">The JSON schema.</param>
    /// <returns>The generated default JSON.</returns>
    public async Task<string> GenerateDefaultJsonFromSchemaAsync(string jsonSchema)
    {
        var schema = await JsonSchema.FromJsonAsync(jsonSchema);
        RemoveReadOnlyProperties(schema);
        var generator = new SampleJsonDataGenerator(_settings);
        var sampleJson = generator.Generate(schema);
        return sampleJson.ToString();
    }

    /// <summary>
    /// Remove read-only properties from the schema.
    /// </summary>
    /// <param name="schema">The JSON schema.</param>
    private void RemoveReadOnlyProperties(JsonSchema schema)
    {
        foreach (var propertyName in _readOnlyProperties)
        {
            schema.Properties.Remove(propertyName);
        }
    }
}
