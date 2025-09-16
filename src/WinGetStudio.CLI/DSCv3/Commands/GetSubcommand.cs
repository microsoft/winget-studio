// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class GetSubcommand : BaseDscSubcommand
{
    public GetSubcommand()
        : base("get", "Get DSC resources")
    {
    }

    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        return Resource.GetAsync(Input);
    }
}
