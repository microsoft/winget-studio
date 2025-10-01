// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using WinGetStudio.CLI.DSCv3.Models;
using WinGetStudio.CLI.DSCv3.Models.FunctionData;
using WinGetStudio.Services.Settings.Contracts;

namespace WinGetStudio.CLI.DSCv3.DscResources;

internal sealed partial class SettingsResource : BaseResource
{
    private readonly IUserSettings _userSettings;
    private readonly IStringLocalizer<SettingsResource> _localizer;
    public const string ResourceName = "settings";

    public SettingsResource(IUserSettings userSettings, IStringLocalizer<SettingsResource> localizer)
        : base(ResourceName)
    {
        _userSettings = userSettings;
        _localizer = localizer;
    }

    /// <inheritdoc/>
    public async override Task<bool> ExportAsync(string input)
    {
        var data = CreateSettingsFunctionData();
        await data.GetAsync();
        WriteJsonOutputLine(data.Output.ToJson());
        return true;
    }

    /// <inheritdoc/>
    public override Task<bool> GetAsync(string input)
    {
        return ExportAsync(input);
    }

    /// <inheritdoc/>
    public async override Task<bool> SetAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            WriteMessageOutputLine(DscMessageLevel.Error, _localizer["DscInputRequired_Message", "set"]);
            return false;
        }

        try
        {
            var data = CreateSettingsFunctionData(input);
            await data.GetAsync();

            // Capture the diff before updating the output
            var diff = data.GetDiffJson();

            // Only call Set if the desired state is different from the current state
            if (!data.TestState())
            {
                data.Output.Settings = data.Input.Settings;
                await data.SetAsync();
            }

            WriteJsonOutputLine(data.Output.ToJson());
            WriteJsonOutputLine(diff);
            return true;
        }
        catch (Exception e)
        {
            WriteMessageOutputLine(DscMessageLevel.Error, _localizer["DscSetFailed_Message", e.Message]);
            return false;
        }
    }

    /// <inheritdoc/>
    public async override Task<bool> TestAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            WriteMessageOutputLine(DscMessageLevel.Error, _localizer["DscInputRequired_Message", "test"]);
            return false;
        }

        var data = CreateSettingsFunctionData(input);
        await data.GetAsync();
        data.Output.InDesiredState = data.TestState();

        WriteJsonOutputLine(data.Output.ToJson());
        WriteJsonOutputLine(data.GetDiffJson());
        return true;
    }

    /// <inheritdoc/>
    public override bool Schema()
    {
        var data = CreateSettingsFunctionData();
        WriteJsonOutputLine(data.Schema());
        return true;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If an output directory is specified, write the manifests to files,
    /// otherwise output them to the console.
    /// </remarks>
    public override bool Manifest(string outputDir)
    {
        var manifest = GenerateManifest();
        if (!string.IsNullOrEmpty(outputDir))
        {
            try
            {
                File.WriteAllText(Path.Combine(outputDir, $"microsoft.wingetstudio.settings.dsc.resource.json"), manifest);
            }
            catch (Exception ex)
            {
                var errorMessage = _localizer["DscManifestFailed_Message", outputDir, ex.Message];
                WriteMessageOutputLine(DscMessageLevel.Error, errorMessage);
                return false;
            }
        }
        else
        {
            WriteJsonOutputLine(manifest);
        }

        return true;
    }

    /// <summary>
    /// Generate a DSC resource JSON manifest.
    /// </summary>
    /// <returns>A JSON string representing the DSC resource manifest.</returns>
    private string GenerateManifest()
    {
        // Note: The description is not localized because the generated
        // manifest file will be part of the package
        return new DscManifest($"Settings", "0.1.0")
            .AddDescription($"Allows management of the settings state via the DSC v3 command line interface protocol.")
            .AddStdinMethod("export", ["dsc", "export", "--resource", "settings"])
            .AddStdinMethod("get", ["dsc", "get", "--resource", "settings"])
            .AddJsonInputMethod("set", "--input", ["dsc", "set", "--resource", "settings"], implementsPretest: true, stateAndDiff: true)
            .AddJsonInputMethod("test", "--input", ["dsc", "test", "--resource", "settings"], stateAndDiff: true)
            .AddCommandMethod("schema", ["dsc", "schema", "--resource", "settings"])
            .ToJson();
    }

    /// <summary>
    /// Creates a new instance of <see cref="SettingsFunctionData"/>.
    /// </summary>
    /// <param name="input">The input string, if any.</param>
    /// <returns>A new instance of <see cref="SettingsFunctionData"/>.</returns>
    private SettingsFunctionData CreateSettingsFunctionData(string input = null)
    {
        return new SettingsFunctionData(_userSettings, input);
    }
}
