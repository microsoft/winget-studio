// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using NJsonSchema.Generation;
using WinGetStudio.CLI.DSCv3.Models.ResourceObjects;

namespace WinGetStudio.CLI.DSCv3.Models.FunctionData;

internal partial class BaseFunctionData
{
    /// <summary>
    /// Generates a JSON schema for the specified resource object type.
    /// </summary>
    /// <typeparam name="T">The type of the resource object.</typeparam>
    /// <returns>A JSON schema string.</returns>
    protected static string GenerateSchema<T>()
        where T : BaseResourceObject
    {
        var settings = new SystemTextJsonSchemaGeneratorSettings()
        {
            FlattenInheritanceHierarchy = true,
            SerializerOptions =
            {
                IgnoreReadOnlyFields = true,
            },
        };
        var generator = new JsonSchemaGenerator(settings);
        var schema = generator.Generate(typeof(T));
        return schema.ToJson(Formatting.None);
    }
}
