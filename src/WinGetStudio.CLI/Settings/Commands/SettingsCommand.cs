// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Windows.System;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.CLI.Settings.Commands;

internal sealed partial class SettingsCommand : Command
{
    public SettingsCommand(IStringLocalizer<SettingsCommand> localizer)
        : base("settings", localizer["Settings_HelpText"])
    {
        SetAction(CommandHandlerAsync);
    }

    public async Task CommandHandlerAsync(ParseResult parseResult)
    {
        await Launcher.LaunchUriAsync(new Uri(RuntimeHelper.GetSettingsFilePath()));
    }
}
