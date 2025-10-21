// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Threading.Tasks;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

public sealed partial class Psm1Parser : IDSCResourceParser
{
    private const string DscResourceAttributeName = "DscResource";
    private const string DscPropertyAttributeName = "DscProperty";

    /// <inheritdoc/>
    public bool CanParse(string fileName) => fileName.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DSCResourceClassDefinition>> ParseAsync(StreamReader streamReader)
    {
        var psm1Content = await streamReader.ReadToEndAsync();
        var ast = Parser.ParseInput(psm1Content, out var tokens, out var errors);
        return [.. ast
            .FindAll(IsDscResource, searchNestedScriptBlocks: true)
            .Cast<TypeDefinitionAst>()
            .Select(dscResourceAst =>
            {
                return new DSCResourceClassDefinition()
                {
                    ClassAst = dscResourceAst,
                    Properties = dscResourceAst.Members.OfType<PropertyMemberAst>().Where(IsDscProperty).ToList(),
                };
            })];
    }

    /// <summary>
    /// Checks if the given AST node represents a DSC Resource.
    /// </summary>
    /// <param name="ast">The AST node to check.</param>
    /// <returns>True if the AST node is a DSC Resource; otherwise, false.</returns>
    private bool IsDscResource(Ast ast)
    {
        return ast is TypeDefinitionAst t && t.Attributes.Any(a => a.TypeName.Name.Equals(DscResourceAttributeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the given property AST node is marked as a DSC Property.
    /// </summary>
    /// <param name="ast">The property AST node to check.</param>
    /// <returns>True if the property is a DSC Property; otherwise, false.</returns>
    private bool IsDscProperty(PropertyMemberAst ast)
    {
        return ast.Attributes.Any(a => a.TypeName.Name.Equals(DscPropertyAttributeName, StringComparison.OrdinalIgnoreCase));
    }
}
