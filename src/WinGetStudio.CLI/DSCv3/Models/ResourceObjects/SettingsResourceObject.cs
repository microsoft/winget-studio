// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.CLI.DSCv3.Models.ResourceObjects;

internal sealed class SettingsResourceObject : BaseResourceObject
{
    public const string SettingsJsonPropertyName = "settings";

    /// <summary>
    /// Gets or sets the settings content.
    /// </summary>
    [JsonPropertyName(SettingsJsonPropertyName)]
    [Required]
    [Description("The settings content.")]
    public GeneralSettings Settings { get; set; } = new();
}
