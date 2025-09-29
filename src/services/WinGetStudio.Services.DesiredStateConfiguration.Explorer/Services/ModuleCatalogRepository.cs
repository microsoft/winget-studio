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
    private readonly IModuleCatalogJsonFileCacheProvider _jsonCacheProvider;
    private readonly IModuleCatalogMemoryCacheProvider _memoryCacheProvider;

    public ModuleCatalogRepository(
        ILogger<ModuleCatalogRepository> logger,
        IEnumerable<IModuleProvider> moduleProviders,
        IModuleCatalogJsonFileCacheProvider jsonCacheProvider,
        IModuleCatalogMemoryCacheProvider memoryCacheProvider)
    {
        _logger = logger;
        _moduleProviders = moduleProviders;
        _jsonCacheProvider = jsonCacheProvider;
        _memoryCacheProvider = memoryCacheProvider;
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
        if (provider.UseCache && _memoryCacheProvider.TryGet(provider.Name, out var inMemory))
        {
            _logger.LogInformation($"Updating cached module catalog for '{provider.Name}' after enriching module '{dscModule.Id}'.");
            await _jsonCacheProvider.SaveCacheAsync(inMemory);
        }
    }

    /// <inheritdoc/>
    public async Task ClearCacheAsync()
    {
        await Parallel.ForEachAsync(_moduleProviders, async (provider, _) =>
        {
            await ClearCacheAsync(provider);
        });
    }

    private async Task ClearCacheAsync(IModuleProvider moduleProvider)
    {
        if (moduleProvider.UseCache)
        {
            _logger.LogInformation($"Clearing cache for module catalog '{moduleProvider.Name}'.");
            _memoryCacheProvider.Remove(moduleProvider.Name);
            await _jsonCacheProvider.ClearCacheAsync(moduleProvider.Name);
        }
    }

    private async Task<DSCModuleCatalog> GetModuleCatalogAsync(string catalogName)
    {
        var moduleProvider = GetModuleProvider(catalogName);

        // 1. Check cache first if enabled
        if (moduleProvider.UseCache)
        {
            // 1.1 Check in-memory cache first
            if (_memoryCacheProvider.TryGet(catalogName, out var inMemory))
            {
                _logger.LogInformation($"Using in-memory cached module catalog for '{catalogName}'.");
                return inMemory;
            }

            // 1.2 Check JSON file cache next
            var jsonCatalog = await _jsonCacheProvider.GetModuleCatalogAsync(catalogName);
            if (jsonCatalog != null)
            {
                _logger.LogInformation($"Using JSON cached module catalog for '{catalogName}'.");
                _memoryCacheProvider.Set(jsonCatalog);
                return jsonCatalog;
            }
        }

        // 2. Fetch from provider
        _logger.LogInformation($"Fetching module catalog for '{catalogName}' from provider.");
        var catalog = await moduleProvider.GetModuleCatalogAsync();

        // 3. Save to cache if enabled
        if (moduleProvider.UseCache && catalog != null)
        {
            _memoryCacheProvider.Set(catalog);
            await _jsonCacheProvider.SaveCacheAsync(catalog);
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
