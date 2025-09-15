// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class SetSubcommand : BaseDscSubcommand
{
    public SetSubcommand()
        : base("set", "Set DSC resources")
    {
    }

    /// <inheritdoc/>
    public override bool CommandHandlerInternal(ParseResult parseResult)
    {
        return Resource.SetState(Input);
    }
}
