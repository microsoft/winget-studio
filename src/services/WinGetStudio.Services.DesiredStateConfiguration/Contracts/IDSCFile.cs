// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;

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
}
