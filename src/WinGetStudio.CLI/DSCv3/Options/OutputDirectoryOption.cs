// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using Microsoft.Extensions.Localization;

namespace WinGetStudio.CLI.DSCv3.Options;

internal sealed class OutputDirectoryOption : Option<string>
{
    private readonly IStringLocalizer<OutputDirectoryOption> _localizer;

    public OutputDirectoryOption(IStringLocalizer<OutputDirectoryOption> localizer)
        : base("--outputDir")
    {
        _localizer = localizer;
        Description = localizer["DscOutputDir_HelpText"];
        Validators.Add(OptionValidator);
    }

    /// <summary>
    /// Validates the output directory option.
    /// </summary>
    /// <param name="result">The option result to validate.</param>
    private void OptionValidator(OptionResult result)
    {
        var value = result.GetValueOrDefault<string>() ?? string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            result.AddError(_localizer["DscOutputDirIsEmpty_HelpText"]);
        }
        else if (!Directory.Exists(value))
        {
            result.AddError(_localizer["DscOutputDirNotFound_HelpText", value]);
        }
    }
}
