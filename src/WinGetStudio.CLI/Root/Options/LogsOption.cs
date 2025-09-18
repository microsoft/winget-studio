// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using WinGetStudio.CLI.Root.Actions;

namespace WinGetStudio.CLI.Root.Options;

internal sealed class LogsOption : Option<bool>
{
    private CommandLineAction _action;

    public LogsOption()
        : base("--logs", [])
    {
        Description = "Opens the logs folder.";
        _action = new LogsOptionAction();
    }

    public override CommandLineAction Action
    {
        get => _action;
        set => _action = value;
    }
}
