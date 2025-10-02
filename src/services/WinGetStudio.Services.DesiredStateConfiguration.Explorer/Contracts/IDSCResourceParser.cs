// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;

public interface IDSCResourceParser
{
    /// <summary>
    /// Determines if the parser can handle the given file based on its name or extension.
    /// </summary>
    /// <param name="fileName">The name of the file to check.</param>
    /// <returns>True if the parser can handle the file; otherwise, false.</returns>
    bool CanParse(string fileName);

    /// <summary>
    /// Parses the provided StreamReader to extract DSC resource class definitions.
    /// </summary>
    /// <param name="streamReader">The StreamReader containing the content to parse.</param>
    /// <returns>A list of DSC resource class definitions</returns>
    Task<IReadOnlyList<DSCResourceClassDefinition>> ParseAsync(StreamReader streamReader);
}
