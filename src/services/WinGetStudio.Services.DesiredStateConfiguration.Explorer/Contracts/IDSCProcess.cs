// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IDSCProcess
{
    /// <summary>
    /// Gets the schema for a given DSC resource.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>The DSC process result containing the schema.</returns>
    Task<DSCProcessResult> GetResourceSchemaAsync(string resource);

    /// <summary>
    /// Gets the current state of a DSC resource.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <param name="input">The input content (JSON or YAML) containing the resource settings.</param>
    /// <returns>The DSC process result containing the current state.</returns>
    Task<DSCProcessResult> GetResourceAsync(string resource, string input);

    /// <summary>
    /// Sets the state of a DSC resource to match the desired state.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <param name="input">The input content (JSON or YAML) containing the desired state.</param>
    /// <returns>The DSC process result containing before/after states and changed properties.</returns>
    Task<DSCProcessResult> SetResourceAsync(string resource, string input);

    /// <summary>
    /// Tests whether a DSC resource is in the desired state.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <param name="input">The input content (JSON or YAML) containing the desired state.</param>
    /// <returns>The DSC process result containing the test results.</returns>
    Task<DSCProcessResult> TestResourceAsync(string resource, string input);
}
