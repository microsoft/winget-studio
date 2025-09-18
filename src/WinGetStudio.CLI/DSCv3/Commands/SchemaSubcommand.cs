// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;
using WinGetStudio.CLI.Contracts;
using WinGetStudio.CLI.DSCv3.Contracts;

namespace WinGetStudio.CLI.DSCv3.Commands;

internal sealed partial class SchemaSubcommand : BaseDscSubcommand
{
    public SchemaSubcommand(IOptionFactory optionFactory, IResourceProvider resourceProvider)
        : base("schema", "Schema description", optionFactory, resourceProvider)
    {
    }

    /// <inheritdoc/>
    protected override Task<bool> CommandHandlerInternalAsync(ParseResult parseResult)
    {
        return Task.FromResult(Resource.Schema());
    }
}
