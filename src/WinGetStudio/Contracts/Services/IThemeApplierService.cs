// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;

namespace WinGetStudio.Contracts.Services;

/// <summary>
/// Service to apply a theme to the application.
/// </summary>
public interface IThemeApplierService
{
    /// <summary>
    /// Applies the specified theme to the application.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ApplyThemeAsync(ElementTheme theme);
}
