// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Versioning;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class DSCModule : IDisposable
{
    private readonly IModuleProvider _provider;
    private readonly Lazy<Task<IReadOnlySet<string>>> _resourceNamesLazy;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private Dictionary<string, DSCResourceClassDefinition> _resourceDefinitions;
    private bool _disposedValue;

    /// <summary>
    /// Gets or sets the module identifier.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the module version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the module description.
    /// </summary>
    public string Tags { get; set; }

    public DSCModule(IModuleProvider provider)
    {
        _provider = provider;
        _resourceNamesLazy = new(() => provider.GetResourceNamesAsync(this));
    }

    /// <summary>
    /// Gets the list of dsc resource names in the module.
    /// </summary>
    /// <returns>A set of resource names.</returns>
    public async Task<IReadOnlySet<string>> GetResourceNamesAsync()
    {
        return await _resourceNamesLazy.Value;
    }

    /// <summary>
    /// Gets the details of a specific DSC resource in the module.
    /// </summary>
    /// <param name="resourceName">The name of the DSC resource.</param>
    /// <returns>The DSC resource details.</returns>
    public async Task<DSCResource> GetResourceAsync(string resourceName)
    {
        await _semaphore.WaitAsync();

        try
        {
            // First time we need to load all resource definitions
            if (_resourceDefinitions == null)
            {
                var definitions = await _provider.GetResourceDefinitionsAsync(this);
                _resourceDefinitions = definitions.ToDictionary(def => def.ClassName, def => def);
            }

            // Now try to get the specific resource
            var resource = new DSCResource() { Name = resourceName };
            if (_resourceDefinitions.TryGetValue(resourceName, out var resourceDefinition))
            {
                // Map properties
                var properties = resourceDefinition.Properties.Select(prop => new DSCProperty
                {
                    Name = prop.Name,
                    Type = prop.PropertyType.TypeName.Name,
                    Syntax = prop.Extent.Text,
                });

                resource.Syntax = resourceDefinition.ClassAst.Extent.Text;
                resource.Properties = [..properties];
            }

            return resource;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _semaphore.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
