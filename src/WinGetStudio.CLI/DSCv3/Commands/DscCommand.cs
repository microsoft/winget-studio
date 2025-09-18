// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using WinGetStudio.CLI.Contracts;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class DscCommand : Command
{
    public DscCommand(ICommandFactory commandFactory)
        : base("dsc", "Manage DSC resources")
    {
        Subcommands.Add(commandFactory.Create<GetSubcommand>());
        Subcommands.Add(commandFactory.Create<SetSubcommand>());
        Subcommands.Add(commandFactory.Create<ExportSubcommand>());
        Subcommands.Add(commandFactory.Create<TestSubcommand>());
        Subcommands.Add(commandFactory.Create<SchemaSubcommand>());
        Subcommands.Add(commandFactory.Create<ManifestSubcommand>());
    }
}
