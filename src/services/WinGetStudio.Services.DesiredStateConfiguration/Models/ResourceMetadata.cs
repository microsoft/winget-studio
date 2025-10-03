// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

public class ResourceMetadata
{
    /// <summary>
    /// Gets or sets the resource type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the resource version.
    /// </summary>
    public string Version { get; set; }
}
