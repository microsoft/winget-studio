// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.Generation;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Helpers;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed partial class DSCResourceJsonSchemaDefaultGenerator : IDSCResourceJsonSchemaDefaultGenerator
{
    private readonly SampleJsonDataGeneratorSettings _settings;
    private readonly HashSet<string> _readOnlyProperties = ["_inDesiredState", "SID"];

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
    public async Task<string> GenerateDefaultYamlFromSchemaAsync(JsonSchema jsonSchema)
    {
        var json = await GenerateDefaultJsonFromSchemaAsync(jsonSchema);
        return DSCYamlHelper.JsonToYaml(json);
    }

    /// <summary>
    /// Generate default JSON from the given JSON schema.
    /// </summary>
    /// <param name="jsonSchema">The JSON schema.</param>
    /// <returns>The generated default JSON.</returns>
    public async Task<string> GenerateDefaultJsonFromSchemaAsync(JsonSchema jsonSchema)
    {
        return await Task.Run(async () =>
        {
            // Clone the schema to avoid modifying the original.
            jsonSchema = await JsonSchema.FromJsonAsync(jsonSchema.ToJson());

            // The generated JSON should not include read-only properties.
            RemoveReadOnlyProperties(jsonSchema);

            // Remove all null types from schema properties and definitions
            // to ensure the generated JSON has a fully populated structure.
            RemoveAllNullTypes(jsonSchema);

            // Generate sample JSON data based on the modified schema.
            var generator = new SampleJsonDataGenerator(_settings);
            var sampleJson = generator.Generate(jsonSchema);
            return sampleJson.ToString();
        });
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

    /// <summary>
    /// Remove all null types from the schema and its subschemas.
    /// </summary>
    /// <param name="root">The root JSON schema.</param>
    private static void RemoveAllNullTypes(JsonSchema root)
    {
        if (root != null)
        {
            var allSchemas = GetAllSchemas(root);
            foreach (var schema in allSchemas)
            {
                RemoveNullTypes(schema);
            }
        }
    }

    /// <summary>
    /// Remove all null types from the given schema.
    /// </summary>
    /// <param name="schema">The JSON schema.</param>
    private static void RemoveNullTypes(JsonSchema schema)
    {
        // If the schema type includes Null, remove it.
        if (schema.Type.HasFlag(JsonObjectType.Null))
        {
            schema.Type &= ~JsonObjectType.Null;
        }

        // Set IsNullableRaw to false to indicate the schema is not nullable.
        schema.IsNullableRaw = false;

        // Remove null-typed members from combinational schemas.
        RemoveNullTypedMembers(schema.OneOf);
        RemoveNullTypedMembers(schema.AllOf);
        RemoveNullTypedMembers(schema.AnyOf);

        // Remove "x-nullable" extension data if present.
        schema.ExtensionData?.Remove("x-nullable");
    }

    /// <summary>
    /// Remove members with null type from the given collection of schemas.
    /// </summary>
    /// <param name="schemas">The collection of JSON schemas.</param>
    private static void RemoveNullTypedMembers(ICollection<JsonSchema> schemas)
    {
        if (schemas == null || schemas.Count == 0)
        {
            return;
        }

        // Identify members to remove.
        var membersToRemove = schemas.Where(schema =>
        {
            schema = schema?.ActualSchema ?? schema;
            if (schema == null)
            {
                return false;
            }

            // A member is considered null-typed if its type includes Null or
            // it is explicitly marked as nullable.
            var includesNull = schema.Type.HasFlag(JsonObjectType.Null);
            var isExplicitlyNullable = schema.IsNullableRaw ?? false;
            return includesNull || isExplicitlyNullable;
        }).ToList();

        // Remove null-typed members.
        foreach (var member in membersToRemove)
        {
            schemas.Remove(member);
        }
    }

    /// <summary>
    /// Get all schemas in the schema tree.
    /// </summary>
    /// <param name="root">The root JSON schema.</param>
    /// <returns>>A list of all JSON schemas.</returns>
    private static List<JsonSchema> GetAllSchemas(JsonSchema root)
    {
        var result = new List<JsonSchema>();
        var stack = new Stack<JsonSchema>();
        var visited = new HashSet<JsonSchema>(ReferenceEqualityComparer.Instance);

        // Traverse the schema tree using depth-first search.
        stack.Push(root);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current != null)
            {
                var schema = current.ActualSchema ?? current;
                if (visited.Add(schema))
                {
                    result.Add(schema);
                    var childrenSchemas = GetChildrenSchemas(schema);
                    foreach (var childSchema in childrenSchemas)
                    {
                        stack.Push(childSchema);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get the child schemas of the given schema.
    /// </summary>
    /// <param name="root">The JSON schema.</param>
    /// <returns>A list of child JSON schemas.</returns>
    private static List<JsonSchema> GetChildrenSchemas(JsonSchema root)
    {
        if (root == null)
        {
            return [];
        }

        return [..Enumerable.Empty<JsonSchema>()
            .Concat(root.AllOf ?? [])
            .Concat(root.AnyOf ?? [])
            .Concat(root.OneOf ?? [])
            .Append(root.Not)
            .Append(root.Item)
            .Concat(root.Items ?? [])
            .Concat(root.Properties?.Values ?? [])
            .Concat(root.PatternProperties?.Values ?? [])
            .Append(root.AdditionalPropertiesSchema)
            .Concat(root.Definitions?.Values ?? [])
            .OfType<JsonSchema>()];
    }
}
