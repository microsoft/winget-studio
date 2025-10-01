// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Contracts;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class SetSubcommand : BaseDscSubcommand
{
    public SetSubcommand(
        IOptionFactory optionFactory,
        IResourceProvider resourceProvider,
        IStringLocalizer<SetSubcommand> localizer)
        : base("set", localizer["DscSet_HelpText"], optionFactory, resourceProvider)
    {
    }

    /// <inheritdoc/>
    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        return Resource.SetAsync(Input);
    }
}
