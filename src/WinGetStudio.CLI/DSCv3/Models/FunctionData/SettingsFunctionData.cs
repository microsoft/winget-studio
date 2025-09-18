// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using WinGetStudio.CLI.DSCv3.Models.ResourceObjects;
using WinGetStudio.Services.Settings.Contracts;
using WinGetStudio.Services.Settings.Models;

namespace WinGetStudio.CLI.DSCv3.Models.FunctionData;

internal sealed partial class SettingsFunctionData : BaseFunctionData
{
    private readonly IUserSettings _userSettings;

    public SettingsResourceObject Input { get; set; }

    public SettingsResourceObject Output { get; set; }

    public SettingsFunctionData(IUserSettings userSettings, string input = null)
    {
        _userSettings = userSettings;

        if (!string.IsNullOrWhiteSpace(input))
        {
            Input = JsonSerializer.Deserialize<SettingsResourceObject>(input);
        }

        Input ??= new();
        Output = new();
    }

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    public async Task GetAsync()
    {
        Output.Settings = await GetSettingsAsync();
    }

    /// <summary>
    /// Sets the current settings.
    /// </summary>
    public Task SetAsync()
    {
        Debug.Assert(Output.Settings != null, "Output settings should not be null");
        return SaveSettingsAsync(Output.Settings);
    }

    /// <summary>
    /// Tests if the current settings and the desired state are valid.
    /// </summary>
    /// <returns>True if the current settings match the desired state; otherwise false.</returns>
    public bool TestState()
    {
        var input = JsonSerializer.SerializeToNode(Input.Settings);
        var output = JsonSerializer.SerializeToNode(Output.Settings);
        return JsonNode.DeepEquals(input, output);
    }

    /// <summary>
    /// Gets the difference between the current settings and the desired state in JSON format.
    /// </summary>
    /// <returns>A JSON array representing the differences.</returns>
    public JsonArray GetDiffJson()
    {
        var diff = new JsonArray();
        if (!TestState())
        {
            diff.Add(SettingsResourceObject.SettingsJsonPropertyName);
        }

        return diff;
    }

    /// <summary>
    /// Gets the schema for the settings resource object.
    /// </summary>
    /// <returns>A JSON schema string.</returns>
    public string Schema()
    {
        return GenerateSchema<SettingsResourceObject>();
    }

    /// <summary>
    /// Gets the settings configuration.
    /// </summary>
    /// <returns>The settings configuration.</returns>
    private Task<GeneralSettings> GetSettingsAsync()
    {
        return Task.FromResult(_userSettings.Current);
    }

    /// <summary>
    /// Saves the settings configuration.
    /// </summary>
    /// <param name="settings">The settings configuration to save.</param>
    private Task SaveSettingsAsync(GeneralSettings settings)
    {
        return _userSettings.SaveAsync(settings);
    }
}
