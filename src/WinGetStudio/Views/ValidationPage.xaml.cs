// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
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

    /// <summary>
    /// Copies the raw data results to the clipboard.
    /// </summary>
    private void CopyResultsToClipboard()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ViewModel.OutputText);
        Clipboard.SetContent(dataPackage);
    }
}
