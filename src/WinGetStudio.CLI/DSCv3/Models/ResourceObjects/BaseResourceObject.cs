// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace WinGetStudio.CLI.DSCv3.Models.ResourceObjects;

internal partial class BaseResourceObject
{
    private readonly JsonSerializerOptions _options;

    public BaseResourceObject()
    {
        _options = new()
        {
            WriteIndented = false,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
        };
    }

    /// <summary>
    /// Gets or sets whether an instance is in the desired state.
    /// </summary>
    [JsonPropertyName("_inDesiredState")]
    [Description("Indicates whether an instance is in the desired state")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? InDesiredState { get; set; }

    /// <summary>
    /// Generates a JSON representation of the resource object.
    /// </summary>
    /// <returns>A JsonNode representing the resource object.</returns>
    public JsonNode ToJson()
    {
        return JsonSerializer.SerializeToNode(this, GetType(), _options) ?? new JsonObject();
    }
}
