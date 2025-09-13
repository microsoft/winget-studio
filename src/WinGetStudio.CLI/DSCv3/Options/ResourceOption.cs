// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace WinGetStudio.CLI.DSCv3.Options;

internal sealed class ResourceOption : Option<string>
{
    private readonly List<string> _resources;

    public ResourceOption(List<string> resources)
        : base("--resource")
    {
        _resources = resources;
        Required = true;
        Description = "The resource to manage.";
        Validators.Add(OptionValidator);
    }

    /// <summary>
    /// Validates the resource option to ensure that the specified resource name is valid.
    /// </summary>
    /// <param name="result">The option result to validate.</param>
    private void OptionValidator(OptionResult result)
    {
        var value = result.GetValueOrDefault<string>() ?? string.Empty;
        if (!_resources.Contains(value))
        {
            result.AddError($"Invalid resource name '{value}'. Valid resources are: {string.Join(", ", _resources)}");
        }
    }
}
