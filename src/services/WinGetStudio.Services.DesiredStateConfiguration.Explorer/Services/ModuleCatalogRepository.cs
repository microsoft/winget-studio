// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class ModuleCatalogRepository : IModuleCatalogRepository
{
    private readonly ILogger<ModuleCatalogRepository> _logger;
    private readonly IEnumerable<IModuleProvider> _moduleProviders;
    private readonly IModuleCatalogJsonFileCacheProvider _cacheProvider;

    public ModuleCatalogRepository(
        ILogger<ModuleCatalogRepository> logger,
        IEnumerable<IModuleProvider> moduleProviders,
        IModuleCatalogJsonFileCacheProvider cacheProvider)
    {
        _logger = logger;
        _moduleProviders = moduleProviders;
        _cacheProvider = cacheProvider;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DSCModuleCatalog>> GetModuleCatalogsAsync()
    {
        List<DSCModuleCatalog> catalogs = [];
        await Parallel.ForEachAsync(_moduleProviders, async (provider, _) =>
        {
            var providerCatalog = await GetModuleCatalogAsync(provider.Name);
            catalogs.Add(providerCatalog);
        });

        return catalogs;
    }

    /// <inheritdoc/>
    public async Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule)
    {
        var provider = GetModuleProvider(dscModule);
        await provider.EnrichModuleWithResourceDetailsAsync(dscModule);
        if (provider.UseCache)
        {
            var cachedCatalog = await _cacheProvider.GetModuleCatalogAsync(provider.Name);
            if (cachedCatalog != null)
            {
                _logger.LogInformation($"Updating cached module catalog for '{provider.Name}' after enriching module '{dscModule.Id}'.");
                await _cacheProvider.SaveCacheAsync(cachedCatalog);
            }
        }
    }

    /// <inheritdoc/>
    public async Task ClearCacheAsync(string catalogName)
    {
        var moduleProvider = GetModuleProvider(catalogName);
        if (moduleProvider.UseCache)
        {
            _logger.LogInformation($"Clearing cache for module catalog '{catalogName}'.");
            await _cacheProvider.ClearCacheAsync(catalogName);
        }
    }

    private async Task<DSCModuleCatalog> GetModuleCatalogAsync(string catalogName)
    {
        var moduleProvider = GetModuleProvider(catalogName);

        // 1. Check cache first if enabled
        if (moduleProvider.UseCache)
        {
            var cachedCatalog = await _cacheProvider.GetModuleCatalogAsync(catalogName);
            if (cachedCatalog != null)
            {
                _logger.LogInformation($"Using cached module catalog for '{catalogName}'.");
                return cachedCatalog;
            }
        }

        // 2. Fetch from provider
        _logger.LogInformation($"Fetching module catalog for '{catalogName}' from provider.");
        var catalog = await moduleProvider.GetModuleCatalogAsync();

        // 3. Save to cache if enabled
        if (moduleProvider.UseCache && catalog != null)
        {
            await _cacheProvider.SaveCacheAsync(catalog);
        }

        return catalog;
    }

    private IModuleProvider GetModuleProvider(string catalogName)
    {
        Debug.Assert(_moduleProviders.Any(p => p.Name == catalogName), "No module provider is registered with the specified catalog name.");
        return _moduleProviders.First(p => p.Name == catalogName);
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

        throw new NotSupportedException();
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
