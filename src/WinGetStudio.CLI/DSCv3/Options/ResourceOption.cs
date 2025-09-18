// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Localization;
using WinGetStudio.CLI.DSCv3.Contracts;

namespace WinGetStudio.CLI.DSCv3.Options;

internal sealed class ResourceOption : Option<string>
{
    private readonly IStringLocalizer<ResourceOption> _localizer;
    private readonly IResourceProvider _resources;

    public ResourceOption(IResourceProvider resources, IStringLocalizer<ResourceOption> localizer)
        : base("--resource")
    {
        _resources = resources;
        _localizer = localizer;
        Required = true;
        Description = localizer["DscResource_HelpText"];
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
            result.AddError(_localizer["DscResourceNotValid_HelpText", value, string.Join(", ", _resources.ResourceNames)]);
        }
    }
}
