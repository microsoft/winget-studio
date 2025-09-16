// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Windows.System;
using Windows.Win32.Foundation;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.CLI.Actions;

internal sealed class LogsOptionAction : SynchronousCommandLineAction
{
    public override int Invoke(ParseResult parseResult)
    {
        var logPath = RuntimeHelper.GetAppLogsPath();
        Launcher.LaunchUriAsync(new Uri(logPath)).Wait();
        return HRESULT.S_OK;
    }
}
