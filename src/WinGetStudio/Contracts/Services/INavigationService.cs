// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WinGetStudio.Contracts.Services;

public interface INavigationService
{
    event NavigatedEventHandler Navigated;

    bool CanGoBack
    {
        get;
    }

    Frame? Frame
    {
        get; set;
    }

    bool NavigateTo(Type pageKey, object? parameter = null, bool clearNavigation = false);

    bool NavigateTo<T>(object? parameter = null, bool clearNavigation = false);

    bool NavigateToDefaultPage(object? parameter = null, bool clearNavigation = false);

    bool GoBack();
}
