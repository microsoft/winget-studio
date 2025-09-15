// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.IO;
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
    public override bool ExportState(string input)
    {
        var data = new SettingsFunctionData();
        data.GetState();
        WriteJsonOutputLine(data.Output.ToJson());
        return true;
    }

    /// <inheritdoc/>
    public override bool GetState(string input)
    {
        return ExportState(input);
    }

    /// <inheritdoc/>
    public override bool SetState(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            WriteMessageOutputLine(DscMessageLevel.Error, "Input cannot be null or empty.");
            return false;
        }

        var data = new SettingsFunctionData(input);
        data.GetState();

        // Capture the diff before updating the output
        var diff = data.GetDiffJson();

        // Only call Set if the desired state is different from the current state
        if (!data.TestState())
        {
            data.Output.Settings = data.Input.Settings;
            data.SetState();
        }

        WriteJsonOutputLine(data.Output.ToJson());
        WriteJsonOutputLine(diff);
        return true;
    }

    /// <inheritdoc/>
    public override bool TestState(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            WriteMessageOutputLine(DscMessageLevel.Error, "Input cannot be null or empty.");
            return false;
        }

        var data = new SettingsFunctionData(input);
        data.GetState();
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
            .AddStdinMethod("export", ["export", "--resource", "settings"])
            .AddStdinMethod("get", ["get", "--resource", "settings"])
            .AddJsonInputMethod("set", "--input", ["set", "--resource", "settings"], implementsPretest: true, stateAndDiff: true)
            .AddJsonInputMethod("test", "--input", ["test", "--resource", "settings"], stateAndDiff: true)
            .AddCommandMethod("schema", ["schema", "--resource", "settings"])
            .ToJson();
    }
}
