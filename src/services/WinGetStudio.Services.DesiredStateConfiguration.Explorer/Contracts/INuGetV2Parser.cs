// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface INuGetV2Parser
{
    /// <summary>
    /// Parses the specified Atom XML feed and extracts metadata for each module.
    /// </summary>
    /// <param name="atomXml">The Atom XML feed as a string.</param>
    /// <returns>A list of module metadata extracted from the feed.</returns>
    IReadOnlyList<ModuleMetadata> ParseFeed(string atomXml);
}
