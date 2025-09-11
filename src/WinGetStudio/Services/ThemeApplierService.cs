// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using WinGetStudio.Contracts.Services;
using WinGetStudio.Helpers;

namespace WinGetStudio.Services;

internal sealed class ThemeApplierService : IThemeApplierService
{
    private readonly IUIDispatcher _dispatcher;

    public ThemeApplierService(IUIDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <inheritdoc/>
    public Task ApplyThemeAsync(ElementTheme theme)
    {
        return _dispatcher.EnqueueAsync(() => SetRequestedThemeAsync(theme));
    }

    /// <summary>
    /// Sets the requested theme for the application's main window.
    /// </summary>
    /// <param name="theme">The heme to apply to the application's main
    /// window.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task SetRequestedThemeAsync(ElementTheme theme)
    {
        if (App.MainWindow.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = theme;
            TitleBarHelper.UpdateTitleBar(theme);
        }

        await Task.CompletedTask;
    }
}
