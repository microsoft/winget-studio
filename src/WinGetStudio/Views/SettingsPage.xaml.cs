// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class SettingsPage : Page, IView<SettingsViewModel>
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
    }

    private void CopyVersionToClipboard(object? sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(ViewModel.VersionDescription))
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(ViewModel.VersionDescription);
            Clipboard.SetContent(dataPackage);
        }
    }
}
