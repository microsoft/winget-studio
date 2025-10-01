// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;
using Windows.Win32.Foundation;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.CLI.Root.Actions;

internal sealed class LogsOptionAction : AsynchronousCommandLineAction
{
    public async override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        // Open the log folder in File Explorer
        var logPath = RuntimeHelper.GetAppLogsPath();
        var result = await Launcher.LaunchUriAsync(new Uri(logPath));
        return result ? HRESULT.S_OK : HRESULT.E_FAIL;
    }
}
