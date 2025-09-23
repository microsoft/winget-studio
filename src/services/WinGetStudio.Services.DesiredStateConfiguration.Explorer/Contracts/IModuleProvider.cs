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
    /// Gets the list of resource names in a specific DSC module.
    /// </summary>
    /// <param name="dscModule">The DSC module to get resource names for.</param>
    /// <returns>A set of resource names.</returns>
    Task<IReadOnlySet<string>> GetResourceNamesAsync(DSCModule dscModule);

    /// <summary>
    /// Gets the definition of all resources in a specific DSC module.
    /// </summary>
    /// <param name="dscModule">The DSC module to get definitions for.</param>
    /// <returns>The list of DSC resource definitions.</returns>
    Task<IReadOnlyList<DSCResourceClassDefinition>> GetResourceDefinitionsAsync(DSCModule dscModule);
}
