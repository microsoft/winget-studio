// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using NJsonSchema;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

internal interface IDSCResourceJsonSchemaDefaultGenerator
{
    /// <summary>
    /// Generates a default YAML configuration from the given JSON schema.
    /// </summary>
    /// <param name="jsonSchema">The JSON schema.</param>
    /// <returns>The generated YAML configuration.</returns>
    Task<string> GenerateDefaultYamlFromSchemaAsync(JsonSchema jsonSchema);

    /// <summary>
    /// Generates a default JSON configuration from the given JSON schema.
    /// </summary>
    /// <param name="jsonSchema">The JSON schema.</param>
    /// <returns>The generated JSON configuration.</returns>
    Task<string> GenerateDefaultJsonFromSchemaAsync(JsonSchema jsonSchema);
}
