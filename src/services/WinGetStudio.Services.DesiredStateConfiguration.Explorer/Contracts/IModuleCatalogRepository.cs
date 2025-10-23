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
    IAsyncEnumerable<DSCModuleCatalog> GetModuleCatalogsAsync();

    /// <summary>
    /// Enriches the given DSC module with detailed resource information.
    /// </summary>
    /// <param name="dscModule">The DSC module to enrich.</param>
    Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule);

    /// <summary>
    /// Generates default YAML configuration for the specified DSC resource.
    /// </summary>
    /// <param name="resource">The DSC resource.</param>
    /// <returns>The generated default YAML configuration.</returns>
    Task<string> GenerateDefaultYamlAsync(DSCResource resource);

    /// <summary>
    /// Clears the cache for all module catalogs.
    /// </summary>
    Task ClearCacheAsync();
}
