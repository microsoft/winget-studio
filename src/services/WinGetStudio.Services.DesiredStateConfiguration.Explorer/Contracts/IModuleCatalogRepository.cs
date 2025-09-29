// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

internal interface IModuleCatalogRepository
{
    /// <summary>
    /// Gets the list of available DSC module catalogs.
    /// </summary>
    /// <returns>A list of DSC module catalogs.</returns>
    Task<IReadOnlyList<DSCModuleCatalog>> GetModuleCatalogsAsync();

    /// <summary>
    /// Enriches the given DSC module with detailed resource information.
    /// </summary>
    /// <param name="dscModule">The DSC module to enrich.</param>
    Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule);

    /// <summary>
    /// Clears the cache for the specified catalog if caching is enabled.
    /// </summary>
    /// <param name="catalogName">The name of the catalog to clear the cache for.</param>
    Task ClearCacheAsync(string catalogName);
}
