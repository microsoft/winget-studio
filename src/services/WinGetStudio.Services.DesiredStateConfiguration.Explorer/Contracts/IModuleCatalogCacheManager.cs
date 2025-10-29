// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

internal interface IModuleCatalogCacheManager
{
    /// <summary>
    /// Gets the cached module catalog for a specific module provider.
    /// </summary>
    /// <param name="moduleProvider">The module provider to get the cache for.</param>
    /// <returns>The cached DSC module catalog, or null if not found.</returns>
    Task<DSCModuleCatalog> GetCacheAsync(IModuleProvider moduleProvider);

    /// <summary>
    /// Updates the cache for a specific module provider.
    /// </summary>
    /// <param name="moduleProvider">The module provider to save the cache for.</param>
    Task UpdateCacheAsync(IModuleProvider moduleProvider);

    /// <summary>
    /// Clears the cache for a specific module provider.
    /// </summary>
    /// <param name="moduleProvider">The module provider to clear the cache for.</param>
    Task ClearCacheAsync(IModuleProvider moduleProvider);

    /// <summary>
    /// Saves the provided module catalog to the cache.
    /// </summary>
    /// <param name="moduleCatalog">The module catalog to save to cache.</param>
    Task SaveCacheAsync(DSCModuleCatalog moduleCatalog);
}
