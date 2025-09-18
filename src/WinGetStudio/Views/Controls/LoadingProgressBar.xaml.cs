// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.ViewModels.Controls;

namespace WinGetStudio.Views.Controls;

public sealed partial class LoadingProgressBar : UserControl
{
    public LoadingProgressBarViewModel ViewModel { get; }

    public LoadingProgressBar()
    {
        ViewModel = App.GetService<LoadingProgressBarViewModel>();
        InitializeComponent();
    }
}
