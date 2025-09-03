// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    /// <returns>A tuple where the first item indicates success, and the second item is the deserialized object (or default if failed).</returns>
    public Task<(bool, T)> TryReadJsonAsync<T>(string filePath, JsonSerializerOptions options = null);

    /// <summary>
    /// Try to serialize an object of type T and save it as a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="filePath">The path to the JSON file.</param>
    /// <param name="content">The object to serialize.</param>
    /// <param name="options">Optional JsonSerializerOptions for serialization.</param>
    /// <returns>True if the operation succeeded, false otherwise.</returns>
    public Task<bool> TrySaveJsonAsync<T>(string filePath, T content, JsonSerializerOptions options = null);

    /// <summary>
    /// Try to delete a file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file to delete.</param>
    /// <returns>True if the operation succeeded, false otherwise.</returns>
    public Task<bool> TryDeleteAsync(string filePath);
}
