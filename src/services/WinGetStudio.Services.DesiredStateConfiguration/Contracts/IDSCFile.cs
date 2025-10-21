// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCFile
{
    /// <summary>
    /// Gets the file info of the configuration file, or null if the file is virtual.
    /// </summary>
    public FileInfo FileInfo { get; }

    /// <summary>
    /// Gets the configuration file content
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets a value indicating whether the configuration file can be saved.
    /// </summary>
    public bool CanSave { get; }

    /// <summary>
    /// Save the configuration file to disk.
    /// </summary>
    /// <param name="localizer">The string localizer.</param>
    /// <exception cref="InvalidDataException">Thrown if the file is virtual and cannot be saved.</exception>
    public Task SaveAsync(IStringLocalizer localizer);
}
