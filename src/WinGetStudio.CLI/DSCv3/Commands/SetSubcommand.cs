// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class SetSubcommand : BaseDscSubcommand
{
    public SetSubcommand()
        : base("set", "Set DSC resources")
    {
    }

    /// <inheritdoc/>
    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        return Resource.SetAsync(Input);
    }
}
