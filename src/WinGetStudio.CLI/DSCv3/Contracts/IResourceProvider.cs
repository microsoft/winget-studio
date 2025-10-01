// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using WinGetStudio.CLI.DSCv3.DscResources;

namespace WinGetStudio.CLI.DSCv3.Contracts;

internal interface IResourceProvider
{
    /// <summary>
    /// Gets a collection of resource names.
    /// </summary>
    IEnumerable<string> ResourceNames { get; }

    /// <summary>
    /// Determines whether a resource with the specified name is available.
    /// </summary>
    /// <param name="resourceName">The name of the resource to check.</param>
    /// <returns>True if the resource is available; otherwise, false.</returns>
    bool IsResourceAvailable(string resourceName);

    /// <summary>
    /// Gets an instance of the resource with the specified name.
    /// </summary>
    /// <param name="name">The name of the resource to get.</param>
    /// <returns>An instance of the resource.</returns>
    BaseResource GetResource(string name);
}
