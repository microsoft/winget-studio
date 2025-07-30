// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace WinGetStudio.Helpers;

public class NavigationHelper
{
    public static Type GetNavigateTo(NavigationViewItem item) => (Type)item.GetValue(NavigateToProperty);

    public static void SetNavigateTo(NavigationViewItem item, Type value) => item.SetValue(NavigateToProperty, value);

    public static readonly DependencyProperty NavigateToProperty =
        DependencyProperty.RegisterAttached("NavigateTo", typeof(Type), typeof(NavigationHelper), new PropertyMetadata(null));
}
