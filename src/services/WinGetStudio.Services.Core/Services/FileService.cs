// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO;
using System.Text;
using System.Text.Json;
using WinGetStudio.Services.Core.Contracts;

namespace WinGetStudio.Services.Core.Services;

public class FileService : IFileService
{
    public bool TryReadJson<T>(string filePath, out T result, JsonSerializerOptions options = null)
    {
        result = default;
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var fileContent = File.ReadAllText(filePath, Encoding.UTF8);
            result = JsonSerializer.Deserialize<T>(fileContent, options);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TrySaveJson<T>(string filePath, T content, JsonSerializerOptions options = null)
    {
        try
        {
            var fileContent = JsonSerializer.Serialize(content, options);
            File.WriteAllText(filePath, fileContent, Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool TryDelete(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
