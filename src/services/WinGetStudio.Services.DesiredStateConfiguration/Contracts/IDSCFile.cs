// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;

namespace WinGetStudio.Services.DesiredStateConfiguration.Contracts;

public interface IDSCFile
{
    /// <summary>
    /// Gets the file info of the configuration file, or null if the file is virtual.
    /// </summary>
    FileInfo FileInfo { get; }

    /// <summary>
    /// Gets the configuration file content
    /// </summary>
    string Content { get; }

    /// <summary>
    /// Gets a value indicating whether the configuration file can be saved.
    /// </summary>
    bool CanSave { get; }

    /// <summary>
    /// Save the configuration file to disk.
    /// </summary>
    /// <param name="localizer">The string localizer.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if the file info is null.</exception>
    Task SaveAsync();
}
