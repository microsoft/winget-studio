// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Threading.Tasks;

namespace WinGetStudio.Services.Core.Contracts;

public interface IFileService
{
    public Task<(bool, T)> TryReadJsonAsync<T>(string filePath, JsonSerializerOptions options = null);

    public Task<bool> TrySaveJsonAsync<T>(string filePath, T content, JsonSerializerOptions options = null);

    public Task<bool> TryDeleteAsync(string filePath);
}
