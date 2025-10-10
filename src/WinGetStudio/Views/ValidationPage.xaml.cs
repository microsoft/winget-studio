// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using WinGetStudio.Contracts.Views;
using WinGetStudio.ViewModels;

namespace WinGetStudio.Views;

public sealed partial class ValidationPage : Page, IView<ValidationViewModel>
{
    private bool _toggle;

    public ValidationViewModel ViewModel { get; }

    public ValidationPage()
    {
        ViewModel = App.GetService<ValidationViewModel>();
        InitializeComponent();
    }

    /// <summary>
    /// Copies the raw data results to the clipboard.
    /// </summary>
    private void CopyResultsToClipboard()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ViewModel.RawData);
        Clipboard.SetContent(dataPackage);
    }

    /// <summary>
    /// Opens the resource explorer dialog for the current resource.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    private async void ExploreResource_Click(object sender, RoutedEventArgs e)
    {
        var resource = await ViewModel.OnExploreAsync();
        if (resource != null)
        {
            ResourceExplorer dialog = new(resource) { XamlRoot = XamlRoot };
            await dialog.ShowAsync();
        }
    }

    private async void Monaco_Get(object sender, RoutedEventArgs e)
    {
        var value = await Editor.GetTextAsync();
        App.GetService<ILogger<ValidationPage>>().LogInformation(value);
    }

    private void Monaco_Set(object sender, RoutedEventArgs e)
    {
        Editor.SetText($"Date: {DateTime.Now}");
    }

    private void Monaco_Language(object sender, RoutedEventArgs e)
    {
        var language = _toggle ? "json" : "yaml";
        _toggle = !_toggle;
        Editor.SetLanguage(language);
    }

    private void Monaco_Theme(object sender, RoutedEventArgs e)
    {
        var theme = _toggle ? "vs-dark" : "vs";
        _toggle = !_toggle;
        Editor.SetTheme(theme);
    }
}
