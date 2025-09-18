// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Contracts;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class GetSubcommand : BaseDscSubcommand
{
    public GetSubcommand(IOptionFactory optionFactory, IResourceProvider resourceProvider)
        : base("get", "Get DSC resources", optionFactory, resourceProvider)
    {
    }

    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        return Resource.GetAsync(Input);
    }
}
