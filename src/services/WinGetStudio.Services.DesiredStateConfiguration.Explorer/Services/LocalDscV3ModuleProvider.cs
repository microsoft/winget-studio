// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

public sealed class LocalDscV3ModuleProvider : IModuleProvider
{
    private readonly IDSC _dsc;

    /// <inheritdoc/>
    public string Name => nameof(DSCModuleSource.LocalDscV3);

    /// <inheritdoc/>
    public bool UseCache => false;

    public LocalDscV3ModuleProvider(IDSC dsc)
    {
        _dsc = dsc;
    }

    /// <inheritdoc/>
    public async Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule)
    {
        // No additional details to enrich for local DSC v3 resources.
        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<GetModuleCatalogResult> GetModuleCatalogAsync()
    {
        var catalog = new DSCModuleCatalog { Name = Name };
        var resources = await _dsc.GetDscV3ResourcesAsync();
        foreach (var resource in resources)
        {
            var module = new DSCModule
            {
                Id = resource.Name,
                Version = resource.Version,
                Source = DSCModuleSource.LocalDscV3,
                IsVirtual = true,
            };
            module.PopulateResources([resource.Name], DSCVersion.V3);
            catalog.Modules.TryAdd(resource.Name, module);
        }

        // For local DSC v3 resources, caching is only enabled if resources are
        // discovered. This workaround addresses a known issue where the WinGet
        // COM API may return an empty resource list.
        // See: https://github.com/PowerShell/DSC/issues/786
        var cache = catalog.Modules?.Count > 0;

        return new()
        {
            CanCache = cache,
            Catalog = catalog,
        };
    }
}
