// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using WinGetStudio.CLI.DSCv3.Contracts;

namespace WinGetStudio.CLI.DSCv3.Options;

internal sealed class ResourceOption : Option<string>
{
    private readonly IResourceProvider _resources;

    public ResourceOption(IResourceProvider resources)
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
        if (!_resources.IsResourceAvailable(value))
        {
            result.AddError($"Invalid resource name '{value}'. Valid resources are: {string.Join(", ", _resources)}");
        }
    }
}
