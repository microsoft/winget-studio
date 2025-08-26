// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace WinGetStudio.Core.Helpers;

public static class Json
{
    public static async Task<T?> ToObjectAsync<T>(string value)
    {
        return await Task.Run(() =>
        {
            return JsonConvert.DeserializeObject<T>(value);
        });
    }

    public static async Task<string> StringifyAsync(object value)
    {
        return await Task.Run<string>(() =>
        {
            return JsonConvert.SerializeObject(value);
        });
    }
}
