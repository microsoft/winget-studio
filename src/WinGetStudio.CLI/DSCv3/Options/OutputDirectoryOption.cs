// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;

namespace WinGetStudio.CLI.DSCv3.Options;

internal sealed class OutputDirectoryOption : Option<string>
{
    public OutputDirectoryOption()
        : base("--outputDir")
    {
        Description = "Path to the output directory.";
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
            result.AddError("Output directory cannot be null or empty");
        }
        else if (!Directory.Exists(value))
        {
            result.AddError($"Output directory does not exist: {value}");
        }
    }
}
