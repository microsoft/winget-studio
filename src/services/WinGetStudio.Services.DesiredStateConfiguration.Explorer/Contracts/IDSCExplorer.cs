// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IDSCExplorer
{
    /// <summary>
    /// Gets the list of DSC module catalogs from all registered providers.
    /// </summary>
    /// <returns>A list of DSC module catalogs.</returns>
    IAsyncEnumerable<DSCModuleCatalog> GetModuleCatalogsAsync();

    /// <inheritdoc cref="IModuleCatalogRepository.EnrichModuleWithResourceDetailsAsync(DSCModule)"/>
    Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule);

    /// <inheritdoc cref="IModuleCatalogRepository.GenerateDefaultYamlAsync(DSCResource)"/>
    Task<string> GenerateDefaultYamlAsync(DSCResource resource);

    /// <inheritdoc cref="IModuleCatalogRepository.ClearCacheAsync"/>
    Task ClearCacheAsync();
}
