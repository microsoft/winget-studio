// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class SchemaSubcommand : BaseDscSubcommand
{
    public SchemaSubcommand()
        : base("schema", "Schema description")
    {
    }

    /// <inheritdoc/>
    public override bool CommandHandlerInternal(ParseResult parseResult)
    {
        return Resource.Schema();
    }
}
