// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinGetStudio.Services.Core.Contracts;

namespace WinGetStudio.Services.Core.Services;

public class FileService : IFileService
{
    /// <inheritdoc/>
    public async Task<ReadJsonResult<T>> TryReadJsonAsync<T>(string filePath, JsonSerializerOptions options = null)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new(false, default, new FileNotFoundException(null, filePath));
            }

            var fileContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            var result = JsonSerializer.Deserialize<T>(fileContent, options);
            return new(true, result);
        }
        catch (Exception ex)
        {
            return new(false, default, ex);
        }
    }

    /// <inheritdoc/>
    public async Task<SaveJsonResult> TrySaveJsonAsync<T>(string filePath, T content, JsonSerializerOptions options = null)
    {
        try
        {
            var fileContent = JsonSerializer.Serialize(content, options);
            await File.WriteAllTextAsync(filePath, fileContent, Encoding.UTF8);
            return new(true);
        }
        catch (Exception ex)
        {
            return new(false, ex);
        }
    }

    /// <inheritdoc/>
    public async Task<DeleteFileResult> TryDeleteAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
            }

            return new(true);
        }
        catch (Exception ex)
        {
            return new(false, ex);
        }
    }

    /// <inheritdoc/>
    public async Task<CopyFileResult> TryCopyAsync(string sourceFilePath, string destinationFilePath, bool overwrite = false)
    {
        try
        {
            if (File.Exists(sourceFilePath))
            {
                await Task.Run(() => File.Copy(sourceFilePath, destinationFilePath, overwrite));
                return new(true);
            }

            return new(false, new FileNotFoundException(null, sourceFilePath));
        }
        catch (Exception ex)
        {
            return new(false, ex);
        }
    }
}
