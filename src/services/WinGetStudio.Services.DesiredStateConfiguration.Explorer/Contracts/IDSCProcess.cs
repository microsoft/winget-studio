// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

internal interface IDSCProcess
{
    /// <summary>
    /// Gets the schema for a given DSC resource.
    /// </summary>
    /// <param name="resource">The resource name.</param>
    /// <returns>The DSC process result containing the schema.</returns>
    Task<DSCProcessResult> GetResourceSchemaAsync(string resource);
}
