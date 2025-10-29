// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using NJsonSchema;
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
    Task<GetModuleCatalogResult> GetModuleCatalogAsync();

    /// <summary>
    /// Enriches the given DSC module with detailed resource information.
    /// </summary>
    /// <param name="dscModule">The DSC module to enrich.</param>
    Task EnrichModuleWithResourceDetailsAsync(DSCModule dscModule);

    /// <summary>
    /// Gets the schema for the given DSC resource.
    /// </summary>
    /// <param name="resource">The DSC resource.</param>
    /// <returns>The resource schema.</returns>
    Task<JsonSchema> GetResourceSchemaAsync(DSCResource resource);
}
