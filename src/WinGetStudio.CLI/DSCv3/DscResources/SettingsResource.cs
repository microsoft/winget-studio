// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using WinGetStudio.CLI.DSCv3.Models;
using WinGetStudio.CLI.DSCv3.Models.FunctionData;

namespace WinGetStudio.CLI.DSCv3.DscResources;

internal sealed partial class SettingsResource : BaseResource
{
    public const string ResourceName = "settings";

    public SettingsResource()
        : base(ResourceName)
    {
    }

    /// <inheritdoc/>
    public async override Task<bool> ExportAsync(string input)
    {
        var data = new SettingsFunctionData();
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
            WriteMessageOutputLine(DscMessageLevel.Error, "Input cannot be null or empty.");
            return false;
        }

        try
        {
            var data = new SettingsFunctionData(input);
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
            WriteMessageOutputLine(DscMessageLevel.Error, $"Failed to set settings: {e.Message}");
            return false;
        }
    }

    /// <inheritdoc/>
    public async override Task<bool> TestAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            WriteMessageOutputLine(DscMessageLevel.Error, "Input cannot be null or empty.");
            return false;
        }

        var data = new SettingsFunctionData(input);
        await data.GetAsync();
        data.Output.InDesiredState = data.TestState();

        WriteJsonOutputLine(data.Output.ToJson());
        WriteJsonOutputLine(data.GetDiffJson());
        return true;
    }

    /// <inheritdoc/>
    public override bool Schema()
    {
        var data = new SettingsFunctionData();
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
                var errorMessage = string.Format(CultureInfo.InvariantCulture, "Failed to write manifest to {0}: {1}", outputDir, ex.Message);
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
}
