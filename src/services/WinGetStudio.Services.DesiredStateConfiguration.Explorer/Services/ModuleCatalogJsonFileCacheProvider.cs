// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WinGetStudio.Services.Core.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

internal sealed class ModuleCatalogJsonFileCacheProvider : IModuleCatalogJsonFileCacheProvider
{
    private readonly string _cacheDirectory;
    private readonly IFileService _fileService;
    private readonly ILogger<ModuleCatalogJsonFileCacheProvider> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly JsonSerializerOptions _jsonOptions;

    public ModuleCatalogJsonFileCacheProvider(
        string cacheDirectory,
        IFileService fileService,
        ILogger<ModuleCatalogJsonFileCacheProvider> logger)
    {
        _cacheDirectory = cacheDirectory;
        _fileService = fileService;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions();
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <inheritdoc/>
    public async Task<DSCModuleCatalog> GetModuleCatalogAsync(string catalogName)
    {
        var sem = GetLock(catalogName);
        await sem.WaitAsync();
        try
        {
            var path = GetCachePath(catalogName);
            _logger.LogInformation($"Attempting to read module catalog from cache at path: {path}");
            var readResult = await _fileService.TryReadJsonAsync<DSCModuleCatalog>(path, _jsonOptions);
            _logger.LogInformation($"Read module catalog from cache result: Success={readResult.Success}, Error={readResult.Error?.Message}");
            return readResult.Success ? readResult.Content : null;
        }
        finally
        {
            sem.Release();
        }
    }

    /// <inheritdoc/>
    public async Task SaveCacheAsync(DSCModuleCatalog catalog)
    {
        var sem = GetLock(catalog.Name);
        await sem.WaitAsync();
        try
        {
            var path = GetCachePath(catalog.Name);
            _logger.LogInformation($"Saving module catalog to cache at path: {path}");
            var saveResult = await _fileService.TrySaveJsonAsync(path, catalog, _jsonOptions);
            _logger.LogInformation($"Save module catalog to cache result: Success={saveResult.Success}, Error={saveResult.Error?.Message}");
        }
        finally
        {
            sem.Release();
        }
    }

    /// <inheritdoc/>
    public async Task ClearCacheAsync(string catalogName)
    {
        var sem = GetLock(catalogName);
        await sem.WaitAsync();
        try
        {
            var path = GetCachePath(catalogName);
            _logger.LogInformation($"Clearing module catalog cache at path: {path}");
            var deleteResult = await _fileService.TryDeleteAsync(path);
            _logger.LogInformation($"Clear module catalog cache result: Success={deleteResult.Success}, Error={deleteResult.Error?.Message}");
        }
        finally
        {
            sem.Release();
        }
    }

    /// <summary>
    /// Get the full cache file path for a given catalog name.
    /// </summary>
    /// <param name="catalogName">The name of the catalog.</param>
    /// <returns>The full path to the cache file.</returns>
    private string GetCachePath(string catalogName) => Path.Combine(_cacheDirectory, $"{catalogName}.json");

    /// <summary>
    /// Get a semaphore lock for a specific catalog name to ensure thread-safe operations.
    /// </summary>
    /// <param name="catalogName">The name of the catalog.</param>
    /// <returns>A SemaphoreSlim instance for the specified catalog name.</returns>
    private SemaphoreSlim GetLock(string catalogName) => _locks.GetOrAdd(catalogName, _ => new(1, 1));
}
