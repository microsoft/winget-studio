// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Linq;
using Microsoft.Extensions.Localization;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Commands;
using WinGetStudio.CLI.Root.Actions;
using WinGetStudio.CLI.Root.Options;
using WinGetStudio.CLI.Settings.Commands;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.CLI.Root.Commands;

internal sealed partial class WinGetStudioCommand : RootCommand
{
    public WinGetStudioCommand(
        ICommandFactory commandFactory,
        IOptionFactory optionFactory,
        IStringLocalizer<WinGetStudioCommand> localizer)
        : base(localizer["WinGetStudioCli_Description", RuntimeHelper.GetAppVersion()])
    {
        // Configure version option
        Options.OfType<VersionOption>().Single().Action = new VersionOptionAction();

        // Commands
        Subcommands.Add(commandFactory.Create<DscCommand>());
        Subcommands.Add(commandFactory.Create<SettingsCommand>());

        // Options
        Options.Add(optionFactory.Create<LogsOption>());
    }
}
