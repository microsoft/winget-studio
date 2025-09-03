// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinGetStudio.Services.Core.Contracts;

namespace WinGetStudio.Services.Core.Services;

public class FileService : IFileService
{
    /// <inheritdoc/>
    public async Task<(bool, T)> TryReadJsonAsync<T>(string filePath, JsonSerializerOptions options = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return (false, default);
            }

            var fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var result = JsonSerializer.Deserialize<T>(fileContent, options);
            return (true, result);
        }
        catch
        {
            return (false, default);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TrySaveJsonAsync<T>(string filePath, T content, JsonSerializerOptions options = null)
    {
        try
        {
            var fileContent = JsonSerializer.Serialize(content, options);
            await File.WriteAllTextAsync(filePath, fileContent, Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TryDeleteAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> TryCopyAsync(string sourceFilePath, string destinationFilePath, bool overwrite = false)
    {
        try
        {
            if (File.Exists(sourceFilePath))
            {
                await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, overwrite));
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
