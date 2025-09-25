// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IModuleProvider
{
    /// <summary>
    /// Gets the name of the module provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the list of DSC modules from the provider.
    /// </summary>
    /// <returns>A list of DSC module identities.</returns>
    Task<DSCModuleCatalog> GetModuleCatalogAsync();

    /// <summary>
    /// Enriches the given DSC module with resource names.
    /// </summary>
    /// <param name="dscModule">The DSC module to enrich.</param>
    Task EnrichModuleWithResourceNamesAsync(DSCModule dscModule);

    /// <summary>
    /// Enriches the given DSC module with detailed resource information.
    /// </summary>
    /// <param name="dscModule">The DSC module to enrich.</param>
    Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule);
}
