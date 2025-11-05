// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WinGetStudio.Contracts.Views;
using WinGetStudio.Services.Core.Helpers;
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
        var dataPackage = new DataPackage();
        var sb = new StringBuilder();

        // This is expected to never be null
        sb.AppendLine(ViewModel.VersionDescription);

        if (RuntimeHelper.TryGetOSVersion(out var osVersion))
        {
            sb.AppendLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "OS Version: {0}",
                    osVersion.ToString()));
        }

        if (RuntimeHelper.TryGetOSArchitecture(out var osArch))
        {
            sb.AppendLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "OS Architecture: {0}",
                    osArch.ToString().ToLowerInvariant()));
        }

        dataPackage.SetText(sb.ToString());
        Clipboard.SetContent(dataPackage);
    }
}
