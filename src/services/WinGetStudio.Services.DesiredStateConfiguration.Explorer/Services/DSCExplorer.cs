// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class DSCExplorer : IDSCExplorer
{
    private readonly IEnumerable<IModuleProvider> _moduleProviders;

    public DSCExplorer(IEnumerable<IModuleProvider> moduleProviders)
    {
        _moduleProviders = moduleProviders;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DSCModuleCatalog>> GetModuleCatalogsAsync()
    {
        List<DSCModuleCatalog> catalogs = [];
        foreach (var provider in _moduleProviders)
        {
            var providerCatalog = await provider.GetModuleCatalogAsync();
            catalogs.Add(providerCatalog);
        }

        return catalogs;
    }

    /// <inheritdoc/>
    public async Task EnrichModuleWithResourceNamesAsync(DSCModule dscModule)
    {
        var provider = GetModuleProvider(dscModule);
        await provider.EnrichModuleWithResourceNamesAsync(dscModule);
    }

    /// <inheritdoc/>
    public async Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule)
    {
        var provider = GetModuleProvider(dscModule);
        await provider.EnrichModuleWithResourceDetailsAsync(dscModule);
    }

    /// <summary>
    /// Gets the appropriate module provider for the specified DSC module.
    /// </summary>
    /// <param name="dscModule">The DSC module to get the provider for.</param>
    /// <returns>>The module provider instance.</returns>
    private IModuleProvider GetModuleProvider(DSCModule dscModule)
    {
        if (dscModule.Source == DSCModuleSource.PSGallery)
        {
            return GetModuleProvider<PowerShellGalleryModuleProvider>();
        }

        if (dscModule.Source == DSCModuleSource.LocalDscV3)
        {
            return GetModuleProvider<LocalDscV3ModuleProvider>();
        }

        throw new InvalidOperationException($"No module provider is registered for source {dscModule.Source}.");
    }

    /// <summary>
    /// Gets the module provider of the specified type.
    /// </summary>
    /// <typeparam name="TModuleProvider">The type of the module provider to get.</typeparam>
    /// <returns>The module provider instance.</returns>
    private IModuleProvider GetModuleProvider<TModuleProvider>()
        where TModuleProvider : IModuleProvider
    {
        Debug.Assert(_moduleProviders.OfType<TModuleProvider>().Any(), $"No module provider of type {typeof(TModuleProvider).FullName} is registered.");
        return _moduleProviders.OfType<TModuleProvider>().First();
    }
}
