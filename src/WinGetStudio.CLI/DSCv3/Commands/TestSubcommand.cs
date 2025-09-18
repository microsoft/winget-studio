// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Contracts;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class TestSubcommand : BaseDscSubcommand
{
    public TestSubcommand(IOptionFactory optionFactory, IResourceProvider resourceProvider)
        : base("test", "Test DSC resources", optionFactory, resourceProvider)
    {
    }

    /// <inheritdoc/>
    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        return Resource.TestAsync(Input);
    }
}
