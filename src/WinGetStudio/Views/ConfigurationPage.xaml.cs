// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class ConfigurationPage : Page, IView<ConfigurationViewModel>
{
    public ConfigurationViewModel ViewModel { get; }

    public ConfigurationPage()
    {
        ViewModel = App.GetService<ConfigurationViewModel>();
        InitializeComponent();

        ViewModel.NavigationService.Frame = NavigationFrame;
    }
}
