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

    public string Name => nameof(DSCModuleSource.LocalDscV3);

    public LocalDscV3ModuleProvider(IDSC dsc)
    {
        _dsc = dsc;
    }

    public async Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule)
    {
        // No additional details to enrich for local DSC v3 resources.
        await Task.CompletedTask;
    }

    public async Task EnrichModuleWithResourceNamesAsync(DSCModule dscModule)
    {
        // No additional details to enrich for local DSC v3 resources.
        await Task.CompletedTask;
    }

    public async Task<DSCModuleCatalog> GetModuleCatalogAsync()
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

        return catalog;
    }
}
