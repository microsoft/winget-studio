// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class ExportSubcommand : BaseDscSubcommand
{
    public ExportSubcommand()
        : base("export", "Export DSC resources")
    {
    }

    public override bool CommandHandlerInternal(ParseResult parseResult)
    {
        return Resource.ExportState(Input);
    }
}
