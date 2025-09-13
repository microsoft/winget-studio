// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;

namespace WinGetStudio.CLI.DSCv3.Options;

internal sealed class InputOption : Option<string>
{
    public InputOption()
        : base("--input")
    {
        Description = "Path to the input file.";
        Validators.Add(OptionValidator);
    }

    /// <summary>
    /// Validates the JSON input provided to the option.
    /// </summary>
    /// <param name="result">The option result to validate.</param>
    private void OptionValidator(OptionResult result)
    {
        var value = result.GetValueOrDefault<string>() ?? string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            result.AddError("Input cannot be null or empty");
        }
        else
        {
            try
            {
                JsonDocument.Parse(value);
            }
            catch (Exception e)
            {
                result.AddError($"Input is not valid JSON: {e.Message}");
            }
        }
    }
}
