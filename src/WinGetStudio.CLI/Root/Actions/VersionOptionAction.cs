// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Invocation;
using Windows.Win32.Foundation;
using WinGetStudio.Services.Core.Helpers;

namespace WinGetStudio.CLI.Root.Actions;

internal sealed class VersionOptionAction : SynchronousCommandLineAction
{
    public override int Invoke(ParseResult parseResult)
    {
        var version = RuntimeHelper.GetAppVersion();
        parseResult.InvocationConfiguration.Output.WriteLine(version);
        return HRESULT.S_OK;
    }
}
