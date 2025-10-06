// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace WinGetStudio.Models;

public sealed class Theme
{
    public string Name { get; }

    public ElementTheme ElementTheme { get; }

    public Theme(string name, ElementTheme elementTheme)
    {
        Name = name;
        ElementTheme = elementTheme;
    }
}
