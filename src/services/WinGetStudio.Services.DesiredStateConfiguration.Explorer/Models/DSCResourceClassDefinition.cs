// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation.Language;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public class DSCResourceClassDefinition
{
    public string ClassName => ClassAst.Name;

    public TypeDefinitionAst ClassAst { get; set; }

    public List<PropertyMemberAst> Properties { get; set; }
}
