// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class ValidationPage : Page, IView<ValidationViewModel>
{
    public ValidationViewModel ViewModel { get; }

    public ValidationPage()
    {
        ViewModel = App.GetService<ValidationViewModel>();
        InitializeComponent();
    }

    private void CopyResultsToClipboard()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ViewModel.RawData);
        Clipboard.SetContent(dataPackage);
    }

    private async void ExploreResource_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var resource = await ViewModel.OnExploreAsync();
        if (resource != null)
        {
            ResourceExplorer dialog = new(resource)
            {
                XamlRoot = XamlRoot,
            };

            await dialog.ShowAsync();
        }
    }
}
