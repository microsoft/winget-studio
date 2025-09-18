// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace WinGetStudio.CLI.DSCv3.Options;

internal sealed class InputOption : Option<string>
{
    private readonly IStringLocalizer<InputOption> _localizer;

    public InputOption(IStringLocalizer<InputOption> localizer)
        : base("--input")
    {
        _localizer = localizer;
        Description = _localizer["DscInput_HelpText"];
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
            result.AddError(_localizer["DscInputIsEmpty_Message"]);
        }
        else
        {
            try
            {
                JsonDocument.Parse(value);
            }
            catch (Exception e)
            {
                result.AddError(_localizer["DscInputNotValid_Message", e.Message]);
            }
        }
    }
}
