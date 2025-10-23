// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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
    /// Populates the resources dictionary with the given resource names.
    /// </summary>
    /// <param name="resourceNames">The resource names.</param>
    public void PopulateResources(IEnumerable<string> resourceNames, DSCVersion dscVersion, DSCModuleSource moduleSource)
    {
        lock (Resources)
        {
            Resources ??= [];
            foreach (var name in resourceNames)
            {
                if (!Resources.ContainsKey(name))
                {
                    Resources[name] = CreateResource(name, dscVersion, moduleSource);
                }
            }
        }
    }

    /// <summary>
    /// Populates the resources dictionary with the given resource class definitions.
    /// </summary>
    /// <param name="classDefinitions">The resource class definitions.</param>
    public void PopulateResources(IEnumerable<DSCResourceClassDefinition> classDefinitions, DSCVersion dscVersion, DSCModuleSource moduleSource)
    {
        lock (Resources)
        {
            Resources ??= [];
            foreach (var definition in classDefinitions)
            {
                if (!Resources.TryGetValue(definition.ClassName, out var resource))
                {
                    resource = CreateResource(definition.ClassName, dscVersion, moduleSource);
                    Resources[definition.ClassName] = resource;
                }

                resource.Syntax = definition.ClassAst.Extent.Text;
                resource.Properties = [.. definition.Properties.Select(prop => new DSCProperty
                {
                    Name = prop.Name,
                    Type = prop.PropertyType.TypeName.Name,
                    Syntax = prop.Extent.Text,
                })];
            }
        }
    }

    /// <summary>
    /// Populates a resource with the given schema.
    /// </summary>
    /// <param name="resourceName">The name of the resource.</param>
    /// <param name="schema">The JSON schema of the resource.</param>
    /// <param name="dscVersion">The DSC version.</param>
    public void PopulateResourceFromSchema(string resourceName, JsonObject schema, DSCVersion dscVersion, DSCModuleSource moduleSource)
    {
        lock (Resources)
        {
            Resources ??= [];
            if (!Resources.TryGetValue(resourceName, out var resource))
            {
                resource = CreateResource(resourceName, dscVersion, moduleSource);
                Resources[resourceName] = resource;
            }

            if (schema?["properties"] is JsonObject properties)
            {
                resource.Syntax = schema.ToJsonString(new() { WriteIndented = false });
                resource.Properties = [..properties.Select(p => new DSCProperty
                {
                    Name = p.Key,
                    Type = p.Value.GetType().Name,
                    Syntax = p.Value.ToJsonString(new() { WriteIndented = false }),
                })];
            }
        }
    }

    /// <summary>
    /// Creates a new DSCResource.
    /// </summary>
    /// <param name="name">The name of the resource.</param>
    /// <returns>The created DSCResource.</returns>
    private DSCResource CreateResource(string name, DSCVersion dscVersion, DSCModuleSource moduleSource)
    {
        return new DSCResource()
        {
            Name = name,
            Version = Version,
            DSCVersion = dscVersion,
            Source = moduleSource,
        };
    }
}
