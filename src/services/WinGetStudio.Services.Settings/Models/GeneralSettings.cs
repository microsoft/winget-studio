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

    /// <inheritdoc/>
    public GeneralSettings Clone()
    {
        return JsonSerializer.Deserialize<GeneralSettings>(JsonSerializer.Serialize(this));
    }

    public override bool Equals(object obj)
    {
        if (obj is GeneralSettings other)
        {
            var thisJson = JsonSerializer.SerializeToElement(this);
            var otherJson = JsonSerializer.SerializeToElement(other);
            return JsonElement.DeepEquals(thisJson, otherJson);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return JsonSerializer.Serialize(this).GetHashCode();
    }
}
