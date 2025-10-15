// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Contracts;

namespace WinGetStudio.Services.DesiredStateConfiguration.Models;

/// <summary>
/// Model class for a YAML configuration file
/// </summary>
public sealed class DSCFile : IDSCFile
{
    public FileInfo FileInfo { get; }

    /// <inheritdoc/>
    public string Content { get; }

    private DSCFile(string content)
        : this(null, content)
    {
    }

    private DSCFile(FileInfo fileInfo, string content)
    {
        FileInfo = fileInfo;
        Content = content;
    }

    /// <summary>
    /// Load a configuration file from a path.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>The configuration file.</returns>
    public static async Task<IDSCFile> LoadAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        using var text = fileInfo.OpenText();
        var content = await text.ReadToEndAsync();
        return new DSCFile(fileInfo, content);
    }

    /// <summary>
    /// Create a virtual file with the specified content without writing to disk.
    /// </summary>
    /// <param name="content">Content of the file</param>
    /// <returns>The configuration file.</returns>
    public static IDSCFile CreateVirtual(string content)
    {
        return new DSCFile(content);
    }
}
