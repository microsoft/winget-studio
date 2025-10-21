// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation.Language;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class DSCResourceClassDefinition
{
    /// <summary>
    /// Gets the name of the class.
    /// </summary>
    public string ClassName => ClassAst.Name;

    /// <summary>
    /// Gets or sets the AST of the class.
    /// </summary>
    public TypeDefinitionAst ClassAst { get; set; }

    /// <summary>
    /// Gets or sets the list of properties in the class.
    /// </summary>
    public List<PropertyMemberAst> Properties { get; set; }
}
