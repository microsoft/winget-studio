// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed class DSCResource
{
    /// <summary>
    /// Gets or sets the name of the resource.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the version of the resource.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Gets the syntax of the resource.
    /// </summary>
    [JsonIgnore]
    public string Syntax => DSCVersion == DSCVersion.V3 ? "json" : "powershell";

    /// <summary>
    /// Gets or sets the syntax of the resource.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the resource source.
    /// </summary>
    [JsonPropertyName("module_source")]
    public DSCModuleSource ModuleSource { get; set; }

    /// <summary>
    /// Gets or sets the properties of the resource.
    /// </summary>
    [JsonPropertyName("properties")]
    public List<DSCProperty> Properties { get; set; } = [];

    /// <summary>
    /// Gets or sets the version of the DSC schema.
    /// </summary>
    [JsonPropertyName("dsc_version")]
    public DSCVersion DSCVersion { get; set; }
}
