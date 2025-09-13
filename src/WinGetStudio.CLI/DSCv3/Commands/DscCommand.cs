// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class DscCommand : BaseDscSubcommand
{
    public DscCommand()
        : base("dsc", "Manage DSC resources")
    {
        Add(new GetSubcommand());
    }
}
