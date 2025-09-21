// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

internal sealed class DSCModule : IDSCModule
{
    private Dictionary<string, DSCResourceClassDefinition> _resourceDefinitions = [];

    public bool IsLocal { get; set; }

    public int DscVersion { get; set; }

    public string Id { get; set; }

    public NuGetVersion Version { get; set; }

    public string Tags { get; set; }

    public IModuleProvider Provider { get; set; }

    public IReadOnlySet<string> Resources { get; set; }

    public DSCModule(IModuleProvider provider)
    {
        Provider = provider;
        Resources = new HashSet<string>();
    }

    public async Task LoadDSCResourcesAsync()
    {
        Resources = await Provider.GetDscModuleResourcesAsync(this);
    }

    public async Task LoadDSCResourcesDefinitionAsync()
    {
        if (_resourceDefinitions.Count == 0)
        {
            var definitions = await Provider.GetDSCModuleResourcesDefinitionAsync(this);
            _resourceDefinitions = definitions.ToDictionary(def => def.ClassName, def => def);
        }
    }

    public DSCResource GetResourceDetails(string resourceName)
    {
        var resource = new DSCResource() { Name = resourceName };
        if (_resourceDefinitions.TryGetValue(resourceName, out var resourceDefinition))
        {
            var properties = resourceDefinition.Properties.ToDictionary(
                prop => prop.Name,
                prop => new DSCProperty
                {
                    Name = prop.Name,
                    Type = prop.PropertyType.TypeName.Name,
                    Syntax = prop.Extent.Text,
                });

            resource.Syntax = resourceDefinition.ClassAst.Extent.Text;
            resource.Properties = properties;
        }

        return resource;
    }
}
