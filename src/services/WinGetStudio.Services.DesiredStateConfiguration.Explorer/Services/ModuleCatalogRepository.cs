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
    private readonly IDSCResourceJsonSchemaDefaultGenerator _generator;
    private readonly IEnumerable<IModuleProvider> _moduleProviders;
    private readonly IModuleCatalogJsonFileCacheProvider _jsonCacheProvider;
    private readonly IModuleCatalogMemoryCacheProvider _memoryCacheProvider;

    public ModuleCatalogRepository(
        ILogger<ModuleCatalogRepository> logger,
        IEnumerable<IModuleProvider> moduleProviders,
        IModuleCatalogJsonFileCacheProvider jsonCacheProvider,
        IModuleCatalogMemoryCacheProvider memoryCacheProvider,
        IDSCResourceJsonSchemaDefaultGenerator generator)
    {
        _logger = logger;
        _moduleProviders = moduleProviders;
        _jsonCacheProvider = jsonCacheProvider;
        _memoryCacheProvider = memoryCacheProvider;
        _generator = generator;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<DSCModuleCatalog> GetModuleCatalogsAsync()
    {
        var tasks = _moduleProviders.Select(async provider => await GetModuleCatalogAsync(provider.Name));
        foreach (var task in tasks)
        {
            yield return await task;
        }
    }

    /// <inheritdoc/>
    public async Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule)
    {
        var provider = GetModuleProvider(dscModule.Source);
        await provider.EnrichModuleWithResourceDetailsAsync(dscModule);
        if (_memoryCacheProvider.TryGet(provider.Name, out var inMemory))
        {
            _logger.LogInformation($"Updating cached module catalog for '{provider.Name}' after enriching module '{dscModule.Id}'.");
            await _jsonCacheProvider.SaveCacheAsync(inMemory);
        }
    }

    /// <inheritdoc/>
    public async Task<string> GenerateDefaultYamlAsync(DSCResource resource)
    {
        var provider = GetModuleProvider(resource.ModuleSource);
        var schema = await provider.GetResourceSchemaAsync(resource);
        if (schema != null)
        {
            return await _generator.GenerateDefaultYamlFromSchemaAsync(schema);
        }
        else
        {
            _logger.LogWarning($"No schema found for resource '{resource.Name}' from source '{resource.ModuleSource}'. Cannot generate sample YAML.");
            return string.Empty;
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

    /// <summary>
    /// Clears the cache for a specific module provider.
    /// </summary>
    /// <param name="moduleProvider">The module provider to clear the cache for.</param>
    private async Task ClearCacheAsync(IModuleProvider moduleProvider)
    {
        _logger.LogInformation($"Clearing cache for module catalog '{moduleProvider.Name}'.");
        _memoryCacheProvider.Remove(moduleProvider.Name);
        await _jsonCacheProvider.ClearCacheAsync(moduleProvider.Name);
    }

    /// <summary>
    /// Gets the module catalog for the specified catalog name, utilizing caching if enabled.
    /// </summary>
    /// <param name="catalogName">The name of the catalog to retrieve.</param>
    /// <returns>>The DSC module catalog.</returns>
    private async Task<DSCModuleCatalog> GetModuleCatalogAsync(string catalogName)
    {
        var moduleProvider = GetModuleProvider(catalogName);

        // 1 Check in-memory cache first
        if (_memoryCacheProvider.TryGet(catalogName, out var inMemory))
        {
            _logger.LogInformation($"Using in-memory cached module catalog for '{catalogName}'.");
            return inMemory;
        }

        // 2 Check JSON file cache next
        var jsonCatalog = await _jsonCacheProvider.GetModuleCatalogAsync(catalogName);
        if (jsonCatalog != null)
        {
            _logger.LogInformation($"Using JSON cached module catalog for '{catalogName}'.");
            _memoryCacheProvider.Set(jsonCatalog);
            return jsonCatalog;
        }

        // 3. Fetch from provider
        _logger.LogInformation($"Fetching module catalog for '{catalogName}' from provider.");
        var result = await moduleProvider.GetModuleCatalogAsync();

        // 4. Save to cache if enabled
        if (result?.CanCache ?? false)
        {
            _memoryCacheProvider.Set(result.Catalog);
            await _jsonCacheProvider.SaveCacheAsync(result.Catalog);
        }

        return result.Catalog;
    }

    /// <summary>
    /// Gets the module provider with the specified catalog name.
    /// </summary>
    /// <param name="catalogName">The name of the catalog.</param>
    /// <returns>>The module provider instance.</returns>
    private IModuleProvider GetModuleProvider(string catalogName)
    {
        Debug.Assert(_moduleProviders.Any(p => p.Name == catalogName), "No module provider is registered with the specified catalog name.");
        return _moduleProviders.First(p => p.Name == catalogName);
    }

    /// <summary>
    /// Gets the appropriate module provider for the specified DSC module.
    /// </summary>
    /// <param name="moduleSource">The source of the DSC module.</param>
    /// <returns>>The module provider instance.</returns>
    private IModuleProvider GetModuleProvider(DSCModuleSource moduleSource)
    {
        if (moduleSource == DSCModuleSource.PSGallery)
        {
            return GetModuleProvider<PowerShellGalleryModuleProvider>();
        }

        if (moduleSource == DSCModuleSource.LocalDscV3)
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
