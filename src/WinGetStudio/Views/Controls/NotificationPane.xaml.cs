// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.ViewModels.Controls;

namespace WinGetStudio.Views.Controls;

public sealed partial class NotificationPane : UserControl
{
    public NotificationPaneViewModel ViewModel { get; }

    public NotificationPane()
    {
        ViewModel = App.GetService<NotificationPaneViewModel>();
        InitializeComponent();
    }
}
