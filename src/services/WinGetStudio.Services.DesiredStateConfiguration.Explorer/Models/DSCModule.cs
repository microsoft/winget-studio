// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using NJsonSchema;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class DSCModule
{
    /// <summary>
    /// Gets or sets the module identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the module version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the module source.
    /// </summary>
    [JsonPropertyName("source")]
    public DSCModuleSource Source { get; set; }

    /// <summary>
    /// Gets or sets the module provider.
    /// </summary>
    [JsonPropertyName("resources")]
    public Dictionary<string, DSCResource> Resources { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether this module has been enriched with resource details.
    /// </summary>
    [JsonPropertyName("is_enriched")]
    public bool IsEnriched { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this module is virtual.
    /// </summary>
    [JsonPropertyName("is_virtual")]
    public bool IsVirtual { get; set; }

    /// <summary>
    /// Adds a new resource to the module.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="dscVersion">The DSC version.</param>
    /// <returns>True if the resource was added; otherwise, false.</returns>
    public bool AddResource(string resourceName, DSCVersion dscVersion)
    {
        lock (Resources)
        {
            Resources ??= [];
            return Resources.TryAdd(resourceName, CreateResource(resourceName, dscVersion));
        }
    }

    /// <summary>
    /// Enriches an existing resource with class definition details.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="classDefinition">The class definition.</param>
    /// <returns>True if the resource was enriched; otherwise, false.</returns>
    public bool EnrichResource(string resourceName, DSCResourceClassDefinition classDefinition)
    {
        lock (Resources)
        {
            if (Resources.TryGetValue(resourceName, out var resource))
            {
                Debug.Assert(resource.DSCVersion != DSCVersion.V3, "DSC v3 resources should be enriched using JSON schema.");
                resource.Code = classDefinition.ClassAst.Extent.Text;
                resource.Properties = [.. classDefinition.Properties.Select(prop => new DSCProperty
                {
                    Name = prop.Name,
                    Type = prop.PropertyType.TypeName.Name,
                    Code = prop.Extent.Text,
                })];
            }

            return resource != null;
        }
    }

    /// <summary>
    /// Enriches an existing resource with JSON schema details.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="schema">The JSON schema.</param>
    /// <returns>True if the resource was enriched; otherwise, false.</returns>
    public bool EnrichResource(string resourceName, JsonSchema schema)
    {
        lock (Resources)
        {
            if (Resources.TryGetValue(resourceName, out var resource))
            {
                Debug.Assert(resource.DSCVersion == DSCVersion.V3, "Only DSC v3 resources should be enriched using JSON schema.");
                resource.Code = schema.ToJson();
                resource.Properties = [..schema.ActualProperties?.Select(prop => new DSCProperty
                {
                    Name = prop.Key,
                    Type = prop.Value.Type.ToString(),
                })];
            }

            return resource != null;
        }
    }

    /// <summary>
    /// Creates a new DSCResource.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <returns>The created DSCResource.</returns>
    private DSCResource CreateResource(string name, DSCVersion dscVersion)
    {
        return new DSCResource()
        {
            Name = name,
            DSCVersion = dscVersion,
            Version = Version,
            ModuleSource = Source,
        };
    }
}
