// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;

public sealed partial class DSCProperty
{
    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the entity or object.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the syntax representation of the object.
    /// </summary>
    public string Syntax { get; set; }

    /// <summary>
    /// Converts the syntax string into a single-line format by removing
    /// whitespace and combining segments.
    /// </summary>
    /// <returns>A single-line syntax representation.</returns>
    public string GetOneLinerSyntax()
    {
        var split = Syntax.Split(['\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(" ", split);
    }
}
