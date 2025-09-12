// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace WinGetStudio.Contracts.Services;

/// <summary>
/// Service to select and apply a theme.
/// </summary>
public interface IThemeSelectorService
{
    /// <summary>
    /// Gets the current application theme.
    /// </summary>
    ElementTheme Theme { get; }

    /// <summary>
    /// Applies the current theme to the application.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ApplyThemeAsync();
}
