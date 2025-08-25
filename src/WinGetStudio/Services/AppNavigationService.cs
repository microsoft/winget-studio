// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Contracts.Services;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Services;

internal sealed class AppNavigationService : NavigationService, IAppNavigationService
{
    public AppNavigationService(IAppPageService pageService)
        : base(pageService)
    {
    }

    public bool NavigateToDefaultPage(object? parameter = null, bool clearNavigation = false)
    {
        return NavigateTo<MainViewModel>(parameter, clearNavigation);
    }

    protected override Frame? GetDefaultFrame()
    {
        return App.MainWindow.Content as Frame;
    }
}
