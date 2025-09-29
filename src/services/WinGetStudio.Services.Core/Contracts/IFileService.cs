// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace WinGetStudio.Services.Core.Contracts;

public interface IFileService
{
    /// <summary>
    /// Try to read a JSON file and deserialize it into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the JSON content into.</typeparam>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <param name="options">Optional JsonSerializerOptions for deserialization.</param>
    /// <returns>The result of the read operation.</returns>
    Task<ReadJsonResult<T>> TryReadJsonAsync<T>(string filePath, JsonSerializerOptions options = null);

    /// <summary>
    /// Try to serialize an object of type T and save it as a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <param name="content">The object to serialize.</param>
    /// <param name="options">Optional JsonSerializerOptions for serialization.</param>
    /// <returns>The result of the save operation.</returns>
    Task<SaveJsonResult> TrySaveJsonAsync<T>(string filePath, T content, JsonSerializerOptions options = null);

    /// <summary>
    /// Try to delete a file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    Task<DeleteFileResult> TryDeleteAsync(string filePath);

    /// <summary>
    /// Try to copy a file from sourceFilePath to destinationFilePath.
    /// </summary>
    /// <param name="sourceFilePath">The path to the source file.</param>
    /// <param name="destinationFilePath">The path to the destination file.</param>
    /// <param name="overwrite">Whether to overwrite the destination file if it already exists.</param>
    /// <returns>The result of the copy operation.</returns>
    Task<CopyFileResult> TryCopyAsync(string sourceFilePath, string destinationFilePath, bool overwrite = false);
}

public record class ReadJsonResult<T>(bool Success, T Content, Exception Error = null);

public record class SaveJsonResult(bool Success, Exception Error = null);

public record class DeleteFileResult(bool Success, Exception Error = null);

public record class CopyFileResult(bool Success, Exception Error = null);
