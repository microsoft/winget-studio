// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class ModuleMetadata
{
    /// <summary>
    /// Gets or sets the module name.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the module version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the module tags.
    /// </summary>
    public string Tags { get; set; }
}
