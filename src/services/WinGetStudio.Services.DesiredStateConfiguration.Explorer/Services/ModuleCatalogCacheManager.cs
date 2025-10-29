// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed partial class ModuleCatalogCacheManager : IModuleCatalogCacheManager
{
    private readonly ILogger<ModuleCatalogCacheManager> _logger;
    private readonly IModuleCatalogJsonFileCacheProvider _jsonCacheProvider;
    private readonly IModuleCatalogMemoryCacheProvider _memoryCacheProvider;

    public ModuleCatalogCacheManager(
        ILogger<ModuleCatalogCacheManager> logger,
        IModuleCatalogJsonFileCacheProvider jsonCacheProvider,
        IModuleCatalogMemoryCacheProvider memoryCacheProvider)
    {
        _logger = logger;
        _jsonCacheProvider = jsonCacheProvider;
        _memoryCacheProvider = memoryCacheProvider;
    }

    /// <inheritdoc/>
    public async Task ClearCacheAsync(IModuleProvider moduleProvider)
    {
        _logger.LogInformation($"Clearing cache for module catalog '{moduleProvider.Name}'.");
        _memoryCacheProvider.Remove(moduleProvider.Name);
        await _jsonCacheProvider.ClearCacheAsync(moduleProvider.Name);
    }

    /// <inheritdoc/>
    public async Task<DSCModuleCatalog> GetCacheAsync(IModuleProvider moduleProvider)
    {
        // 1. Check in-memory cache first
        _logger.LogInformation($"Retrieving cache for module catalog '{moduleProvider.Name}'.");
        if (_memoryCacheProvider.TryGet(moduleProvider.Name, out var inMemory))
        {
            _logger.LogInformation($"Using in-memory cached module catalog for '{moduleProvider.Name}'.");
            return inMemory;
        }

        // 2. Check JSON file cache next
        var jsonCatalog = await _jsonCacheProvider.GetModuleCatalogAsync(moduleProvider.Name);
        if (jsonCatalog != null)
        {
            _logger.LogInformation($"Using JSON cached module catalog for '{moduleProvider.Name}' and loading into in-memory cache.");
            _memoryCacheProvider.Set(jsonCatalog);
            return jsonCatalog;
        }

        // No cache found
        return null;
    }

    /// <inheritdoc/>
    public async Task UpdateCacheAsync(IModuleProvider moduleProvider)
    {
        if (_memoryCacheProvider.TryGet(moduleProvider.Name, out var inMemory))
        {
            _logger.LogInformation($"Updating JSON cache for module catalog '{moduleProvider.Name}'.");
            await _jsonCacheProvider.SaveCacheAsync(inMemory);
        }
        else
        {
            _logger.LogWarning($"No in-memory cache found for module catalog '{moduleProvider.Name}'. Skipping update to JSON cache.");
        }
    }

    /// <inheritdoc/>
    public async Task SaveCacheAsync(DSCModuleCatalog moduleCatalog)
    {
        _logger.LogInformation($"Saving module catalog '{moduleCatalog.Name}' to cache.");
        _memoryCacheProvider.Set(moduleCatalog);
        await _jsonCacheProvider.SaveCacheAsync(moduleCatalog);
    }
}
