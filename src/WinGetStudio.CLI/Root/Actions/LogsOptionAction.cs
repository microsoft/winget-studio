// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using Windows.System;
using Windows.Win32.Foundation;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.CLI.Root.Actions;

internal sealed class LogsOptionAction : SynchronousCommandLineAction
{
    public override int Invoke(ParseResult parseResult)
    {
        // Open the log folder in File Explorer
        var logPath = RuntimeHelper.GetAppLogsPath();
        var result = Launcher.LaunchUriAsync(new Uri(logPath)).GetResults();
        return result ? HRESULT.S_OK : HRESULT.E_FAIL;
    }
}
