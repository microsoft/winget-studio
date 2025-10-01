// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Contracts;
using WinGetStudio.CLI.DSCv3.Options;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class ManifestSubcommand : BaseDscSubcommand
{
    /// <summary>
    /// Option to specify the output directory for the manifest.
    /// </summary>
    private readonly OutputDirectoryOption _outputDirectoryOption;

    public ManifestSubcommand(
        IOptionFactory optionFactory,
        IResourceProvider resourceProvider,
        IStringLocalizer<ManifestSubcommand> localizer)
        : base("manifest", localizer["DscManifest_HelpText"], optionFactory, resourceProvider)
    {
        _outputDirectoryOption = optionFactory.Create<OutputDirectoryOption>();
        Options.Add(_outputDirectoryOption);
    }

    /// <inheritdoc/>
    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        var outputDir = parseResult.GetValue(_outputDirectoryOption);
        return Task.FromResult(Resource.Manifest(outputDir));
    }
}
