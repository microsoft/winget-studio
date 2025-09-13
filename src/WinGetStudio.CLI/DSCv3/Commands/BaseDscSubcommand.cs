// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using WinGetStudio.CLI.DSCv3.DscResources;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal abstract class BaseDscSubcommand : Command
{
    /// <summary>
    /// Gets the DSC resource to be used by the command.
    /// </summary>
    protected BaseResource Resource { get; private set; }

    /// <summary>
    /// Gets the input JSON provided by the user.
    /// </summary>
    protected string Input { get; private set; }

    public BaseDscSubcommand(string name, string description)
        : base(name, description)
    {
            SetAction(CommandHandler);
    }

    protected virtual void CommandHandler(ParseResult parseResult)
    {
        // No-op
    }
}
