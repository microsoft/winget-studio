// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class OpenPackageResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the package was opened successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the package stream.
    /// </summary>
    public Stream PackageStream { get; set; }
}
