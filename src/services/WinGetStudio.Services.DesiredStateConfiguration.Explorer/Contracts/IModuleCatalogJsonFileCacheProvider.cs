// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

internal interface IModuleCatalogJsonFileCacheProvider
{
    /// <summary>
    /// Gets the module catalog from the cache.
    /// </summary>
    /// <param name="catalogName">The name of the catalog.</param>
    /// <returns>The module catalog if found; otherwise, null.</returns>
    Task<DSCModuleCatalog> GetModuleCatalogAsync(string catalogName);

    /// <summary>
    /// Saves the module catalog to the cache.
    /// </summary>
    /// <param name="catalog">The module catalog to save.</param>
    Task SaveCacheAsync(DSCModuleCatalog catalog);

    /// <summary>
    /// Clears the cache for the specified catalog.
    /// </summary>
    /// <param name="catalogName">The name of the catalog.</param>
    Task ClearCacheAsync(string catalogName);
}
