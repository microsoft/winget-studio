// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.ViewModels.Controls;

namespace WinGetStudio.Views.Controls;

public sealed partial class ActivityPane : UserControl
{
    public ActivityPaneViewModel ViewModel { get; }

    public ActivityPane()
    {
        ViewModel = App.GetService<ActivityPaneViewModel>();
        InitializeComponent();
    }
}
