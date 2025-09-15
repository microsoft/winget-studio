// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class DscCommand : Command
{
    public DscCommand()
        : base("dsc", "Manage DSC resources")
    {
        Subcommands.Add(new GetSubcommand());
        Subcommands.Add(new SetSubcommand());
        Subcommands.Add(new ExportSubcommand());
        Subcommands.Add(new TestSubcommand());
        Subcommands.Add(new SchemaSubcommand());
        Subcommands.Add(new ManifestSubcommand());
    }
}
