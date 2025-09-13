// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using WinGetStudio.CLI.DSCv3.Models.ResourceObjects;

namespace WinGetStudio.CLI.DSCv3.Models.FunctionData;

internal sealed partial class SettingsFunctionData : BaseFunctionData
{
    public SettingsResourceObject Input { get; set; }

    public SettingsResourceObject Output { get; set; }

    public SettingsFunctionData(string input)
    {
        if (!string.IsNullOrWhiteSpace(input))
        {
            Input = JsonSerializer.Deserialize<SettingsResourceObject>(input);
        }

        Input ??= new();
        Output = new();
    }
}
