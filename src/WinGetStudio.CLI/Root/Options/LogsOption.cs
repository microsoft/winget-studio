// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Localization;
using WinGetStudio.CLI.Root.Actions;

namespace WinGetStudio.CLI.Root.Options;

internal sealed class LogsOption : Option<bool>
{
    private CommandLineAction _action;

    public LogsOption(IStringLocalizer<LogsOption> localizer)
        : base("--logs", [])
    {
        Description = localizer["Logs_HelpText"];
        _action = new LogsOptionAction();
    }

    public override CommandLineAction Action
    {
        get => _action;
        set => _action = value;
    }
}
