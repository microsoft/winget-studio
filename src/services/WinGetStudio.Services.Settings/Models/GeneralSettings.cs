// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace WinGetStudio.Services.Settings.Models;

public class GeneralSettings
{
    public const string DefaultTheme = "Default";

    [JsonPropertyName("theme")]
    public string Theme { get; set; }

    [JsonPropertyName("telemetry")]
    public TelemetrySettings Telemetry { get; set; }

    public GeneralSettings()
    {
        // Initialize with default values
        Theme = DefaultTheme;
        Telemetry = new TelemetrySettings();
    }

    /// <summary>
    /// Clones the current settings.
    /// </summary>
    /// <returns>A deep copy of the current settings.</returns>
    public GeneralSettings Clone()
    {
        return JsonSerializer.Deserialize<GeneralSettings>(JsonSerializer.Serialize(this));
    }

    /// <summary>
    /// Compares two settings objects for equality.
    /// </summary>
    /// <param name="obj">The other settings object.</param>
    /// <returns>True if the settings are equal, false otherwise.</returns>
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

    /// <summary>
    /// Gets the hash code of the settings object.
    /// </summary>
    /// <returns>The hash code of the settings object.</returns>
    public override int GetHashCode()
    {
        return JsonSerializer.Serialize(this).GetHashCode();
    }
}
