// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class DSCModuleCatalog
{
    /// <summary>
    /// Gets or sets the name of the module catalog.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the collection of DSC modules associated with the current configuration.
    /// </summary>
    [JsonPropertyName("modules")]
    public Dictionary<string, DSCModule> Modules { get; set; } = [];
}
