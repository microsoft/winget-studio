// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface INuGetV2Client
{
    /// <summary>
    /// Searches for modules from a NuGet V2 feed.
    /// </summary>
    /// <param name="baseUrl">The base URL of the NuGet V2 feed.</param>
    /// <param name="query">The search query.</param>
    /// <returns>A list of module metadata matching the search query.</returns>
    Task<IReadOnlyList<ModuleMetadata>> SearchAsync(string baseUrl, NameValueCollection query);
}
