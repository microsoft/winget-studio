// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace WinGetStudio.Models;

/// <summary>
/// Represents a theme option for the application.
/// </summary>
public sealed class Theme
{
    /// <summary>
    /// Gets the name of the theme.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the corresponding ElementTheme value.
    /// </summary>
    public ElementTheme ElementTheme { get; }

    public Theme(string name, ElementTheme elementTheme)
    {
        Name = name;
        ElementTheme = elementTheme;
    }
}
