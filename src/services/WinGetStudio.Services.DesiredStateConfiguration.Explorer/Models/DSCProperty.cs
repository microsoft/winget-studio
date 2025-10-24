// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class DSCProperty
{
    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the entity or object.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the code syntax for the property.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// Converts the code syntax string into a single-line format by removing
    /// whitespace and combining segments.
    /// </summary>
    /// <returns>A single-line code syntax representation.</returns>
    public string GetOneLinerCode()
    {
        var split = Code?.Split(['\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(" ", split ?? []);
    }
}
