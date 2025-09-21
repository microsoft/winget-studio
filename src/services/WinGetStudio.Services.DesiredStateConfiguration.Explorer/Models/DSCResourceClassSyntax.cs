// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Management.Automation.Language;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

internal class DSCResourceClassSyntax
{
    public string ClassName { get; set; }

    public IReadOnlyList<PropertyMemberAst> Properties { get; set; }
}
