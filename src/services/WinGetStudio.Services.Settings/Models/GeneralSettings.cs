// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using WinGetStudio.Services.Settings.Contracts;

namespace WinGetStudio.Services.Settings.Models;

public class GeneralSettings : IGeneralSettings
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; }

    public GeneralSettings Clone()
    {
        return JsonSerializer.Deserialize<GeneralSettings>(JsonSerializer.Serialize(this));
    }
}
