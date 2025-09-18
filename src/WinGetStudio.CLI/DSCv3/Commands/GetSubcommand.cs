// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Contracts;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class GetSubcommand : BaseDscSubcommand
{
    public GetSubcommand(
        IOptionFactory optionFactory,
        IResourceProvider resourceProvider,
        IStringLocalizer<GetSubcommand> localizer)
        : base("get", localizer["DscGet_HelpText"], optionFactory, resourceProvider)
    {
    }

    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        return Resource.GetAsync(Input);
    }
}
