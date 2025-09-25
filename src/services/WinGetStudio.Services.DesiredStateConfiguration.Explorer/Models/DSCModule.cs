// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
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
    /// Populates the resources dictionary with the given resource names.
    /// </summary>
    /// <param name="resourceNames">The resource names.</param>
    public void PopulateResources(IEnumerable<string> resourceNames)
    {
        lock (Resources)
        {
            Resources ??= [];
            foreach (var name in resourceNames)
            {
                if (!Resources.ContainsKey(name))
                {
                    Resources[name] = new DSCResource() { Name = name };
                }
            }
        }
    }

    /// <summary>
    /// Populates the resources dictionary with the given resource class definitions.
    /// </summary>
    /// <param name="classDefinitions">The resource class definitions.</param>
    public void PopulateResources(IEnumerable<DSCResourceClassDefinition> classDefinitions)
    {
        lock (Resources)
        {
            Resources ??= [];
            foreach (var definition in classDefinitions)
            {
                if (!Resources.TryGetValue(definition.ClassName, out var resource))
                {
                    resource = new DSCResource() { Name = definition.ClassName };
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
}
