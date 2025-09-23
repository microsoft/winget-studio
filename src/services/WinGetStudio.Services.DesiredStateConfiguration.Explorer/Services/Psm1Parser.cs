// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Contracts;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Services;

public sealed partial class Psm1Parser : IPsm1Parser
{
    private const string DscResourceAttributeName = "DscResource";
    private const string DscPropertyAttributeName = "DscProperty";

    /// <inheritdoc/>
    public IReadOnlyList<DSCResourceClassDefinition> ParseDscResources(string psm1Content)
    {
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

    private bool IsDscResource(Ast ast)
    {
        return ast is TypeDefinitionAst t && t.Attributes.Any(a => a.TypeName.Name.Equals(DscResourceAttributeName, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsDscProperty(PropertyMemberAst ast)
    {
        return ast.Attributes.Any(a => a.TypeName.Name.Equals(DscPropertyAttributeName, StringComparison.OrdinalIgnoreCase));
    }
}
