// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class TestSubcommand : BaseDscSubcommand
{
    public TestSubcommand()
        : base("test", "Test DSC resources")
    {
    }

    /// <inheritdoc/>
    public override bool CommandHandlerInternal(ParseResult parseResult)
    {
        return Resource.TestState(Input);
    }
}
