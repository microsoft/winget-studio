// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IPsm1Parser
{
    /// <summary>
    /// Parses the content of a .psm1 file and extracts DSC resource definitions.
    /// </summary>
    /// <param name="psm1Content">The content of the .psm1 file.</param>
    /// <returns>A list of DSC resource class definitions
    IReadOnlyList<DSCResourceClassDefinition> ParseDscResources(string psm1Content);
}
