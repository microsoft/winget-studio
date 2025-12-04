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

    /// <summary>
    /// Copies the yaml output results to the clipboard.
    /// </summary>
    private void CopyYamlResultsToClipboard()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ViewModel.SelectedUnit?.YamlOutput);
        Clipboard.SetContent(dataPackage);
    }

    /// <summary>
    /// Copies the error output results to the clipboard.
    /// </summary>
    private void CopyErrorResultsToClipboard()
    {
        var dataPackage = new DataPackage();
        dataPackage.SetText(ViewModel.SelectedUnit?.ErrorOutput);
        Clipboard.SetContent(dataPackage);
    }

    private void UnitValidation_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is ValidateUnitViewModel unitViewModel)
        {
            ViewModel.RemoveUnitValidation(unitViewModel);
        }
    }
}
