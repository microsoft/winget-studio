// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IDSCExplorer
{
    /// <summary>
    /// Gets the list of DSC modules from all registered module providers.
    /// </summary>
    /// <returns>>A list of DSC module catalogs.</returns>
    Task<IReadOnlyList<DSCModuleCatalog>> GetCatalogsAsync();

    /// <summary>
    /// Gets a DSC module catalog from a specific module provider.
    /// </summary>
    /// <typeparam name="TModuleProvider">The type of the module provider.</typeparam>
    /// <returns>>The DSC module catalog.</returns>
    Task<DSCModuleCatalog> GetCatalogAsync<TModuleProvider>()
        where TModuleProvider : IModuleProvider;
}
