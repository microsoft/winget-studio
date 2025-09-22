// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed class DSCProperty
{
    public string Name { get; set; }

    public string Type { get; set; }

    public string Syntax { get; set; }

    public string GetOneLinerSyntax()
    {
        var split = Syntax.Split(['\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(" ", split);
    }
}
