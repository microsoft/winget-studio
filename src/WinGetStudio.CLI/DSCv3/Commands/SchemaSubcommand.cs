// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class SchemaSubcommand : BaseDscSubcommand
{
    public SchemaSubcommand()
        : base("schema", "Schema description")
    {
    }

    /// <inheritdoc/>
    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        return Task.FromResult(Resource.Schema());
    }
}
