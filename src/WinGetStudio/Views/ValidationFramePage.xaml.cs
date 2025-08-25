// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class ValidationFramePage : Page, IView<ValidationFrameViewModel>
{
    public ValidationFrameViewModel ViewModel { get; }

    public ValidationFramePage()
    {
        ViewModel = App.GetService<ValidationFrameViewModel>();
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
    }
}
