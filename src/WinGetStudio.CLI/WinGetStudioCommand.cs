// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Linq;
using WinGetStudio.CLI.Actions;
using WinGetStudio.CLI.DSCv3.Commands;
using WinGetStudio.CLI.Options;
using WinGetStudio.CLI.Settings.Commands;

namespace WinGetStudio.CLI;

internal sealed partial class WinGetStudioCommand : RootCommand
{
    public WinGetStudioCommand()
        : base("WinGetStudio Command Line Interface")
    {
        // Configure version option
        Options.OfType<VersionOption>().Single().Action = new VersionOptionAction();

        // Commands
        Subcommands.Add(new DscCommand());
        Subcommands.Add(new SettingsCommand());

        // Options
        Options.Add(new LogsOption());
    }
}
