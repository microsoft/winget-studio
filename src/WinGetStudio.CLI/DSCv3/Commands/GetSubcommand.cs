// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class GetSubcommand : BaseDscSubcommand
{
    public GetSubcommand()
        : base("get", "Get DSC resources")
    {
    }

    protected override void CommandHandler(ParseResult parseResult)
    {
        Console.Write("DSC get command");
    }
}
