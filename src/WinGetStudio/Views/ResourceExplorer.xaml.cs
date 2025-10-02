// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WinGetStudio.Services.DesiredStateConfiguration.Explorer.Models;
using WinGetStudio.ViewModels.Controls;

namespace WinGetStudio.Views;

public sealed partial class ResourceExplorer : ContentDialog
{
    public ResourceExplorerViewModel ViewModel { get; }

    public ResourceExplorer(DSCResource resource)
    {
        ViewModel = App.GetService<ResourceExplorerViewModel>();
        ViewModel.SetResource(resource);
        InitializeComponent();
    }

    /// <summary>
    /// Copies the property name to the clipboard when the copy button is clicked.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">The event data.</param>
    private void OnCopyPropertyName(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string propertyName)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(propertyName);
            Clipboard.SetContent(dataPackage);
        }
    }

    /// <summary>
    /// Closes the dialog when the close button is clicked.
    /// </summary>
    /// <param name="sender">The button that was clicked.</param>
    /// <param name="e">>The event data.</param>
    private void OnClose(object sender, RoutedEventArgs e) => Hide();
}
