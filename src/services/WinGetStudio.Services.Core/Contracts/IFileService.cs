// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace WinGetStudio.Services.Core.Contracts;

public interface IFileService
{
    public bool TryReadJson<T>(string filePath, out T result, JsonSerializerOptions options = null);

    public bool TrySaveJson<T>(string filePath, T content, JsonSerializerOptions options = null);

    public bool TryDelete(string filePath);
}
