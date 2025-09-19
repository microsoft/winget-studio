// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading.Tasks;

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
    Task<IReadOnlyList<IDSCModule>> GetDSCModulesAsync();

    /// <summary>
    /// Gets the list of DSC resources for a specific module.
    /// </summary>
    /// <param name="dscModule">The DSC module to get resources for.</param>
    /// <returns>A list of DSC resource names.</returns>
    Task<IReadOnlyList<string>> GetDscModuleResourcesAsync(IDSCModule dscModule);
}
