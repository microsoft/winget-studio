// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace WinGetStudio.Services.Settings.Models;

public class TelemetrySettings
{
    [JsonPropertyName("disable")]
    public bool Disable { get; set; }

    public TelemetrySettings()
    {
        // Initialize with default values
        Disable = true;
    }
}
